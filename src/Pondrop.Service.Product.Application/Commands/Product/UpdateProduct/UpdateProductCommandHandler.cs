using AutoMapper;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Pondrop.Service.Product.Application.Interfaces;
using Pondrop.Service.Product.Application.Interfaces.Services;
using Pondrop.Service.Product.Application.Models;
using Pondrop.Service.Product.Domain.Events;
using Pondrop.Service.Product.Domain.Events.Product;
using Pondrop.Service.Product.Domain.Models;
using Pondrop.Service.Product.Domain.Models.Product;

namespace Pondrop.Service.Product.Application.Commands;

public class UpdateProductCommandHandler : DirtyCommandHandler<ProductEntity, UpdateProductCommand, Result<ProductRecord>>
{
    private readonly IEventRepository _eventRepository;
    private readonly ICheckpointRepository<ProductEntity> _ProductCheckpointRepository;
    private readonly IMapper _mapper;
    private readonly IUserService _userService;
    private readonly IValidator<UpdateProductCommand> _validator;
    private readonly ILogger<UpdateProductCommandHandler> _logger;

    public UpdateProductCommandHandler(
        IOptions<ProductUpdateConfiguration> ProductUpdateConfig,
        IEventRepository eventRepository,
        ICheckpointRepository<ProductEntity> ProductCheckpointRepository,
        IDaprService daprService,
        IUserService userService,
        IMapper mapper,
        IValidator<UpdateProductCommand> validator,
        ILogger<UpdateProductCommandHandler> logger) : base(eventRepository, ProductUpdateConfig.Value, daprService, logger)
    {
        _eventRepository = eventRepository;
        _ProductCheckpointRepository = ProductCheckpointRepository;
        _mapper = mapper;
        _userService = userService;
        _validator = validator;
        _logger = logger;
    }

    public override async Task<Result<ProductRecord>> Handle(UpdateProductCommand command, CancellationToken cancellationToken)
    {
        var validation = _validator.Validate(command);

        if (!validation.IsValid)
        {
            var errorMessage = $"Update Product failed, errors on validation {validation}";
            _logger.LogError(errorMessage);
            return Result<ProductRecord>.Error(errorMessage);
        }

        var result = default(Result<ProductRecord>);

        try
        {
            var ProductEntity = await _ProductCheckpointRepository.GetByIdAsync(command.Id);
            ProductEntity ??= await GetFromStreamAsync(command.Id);

            if (ProductEntity is not null)
            {
                var evtPayload = new UpdateProduct(
                    command.Name,
                    command.BrandId,
                    command.ExternalReferenceId,
                    command.Variant,
                    command.AltName,
                    command.ShortDescription,
                    command.NetContent,
                    command.NetContentUom,
                    command.PossibleCategories,
                    command.PublicationLifecycleId,
                    command.ChildProductId ?? new List<Guid>());
                var createdBy = _userService.CurrentUserId();

                var success = await UpdateStreamAsync(ProductEntity, evtPayload, createdBy);

                if (!success)
                {
                    await _ProductCheckpointRepository.FastForwardAsync(ProductEntity);
                    success = await UpdateStreamAsync(ProductEntity, evtPayload, createdBy);
                }

                await Task.WhenAll(
                    InvokeDaprMethods(ProductEntity.Id, ProductEntity.GetEvents(ProductEntity.AtSequence)));

                result = success
                    ? Result<ProductRecord>.Success(_mapper.Map<ProductRecord>(ProductEntity))
                    : Result<ProductRecord>.Error(FailedToCreateMessage(command));
            }
            else
            {
                result = Result<ProductRecord>.Error($"Product does not exist '{command.Id}'");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, FailedToCreateMessage(command));
            result = Result<ProductRecord>.Error(ex);
        }

        return result;
    }

    private static string FailedToCreateMessage(UpdateProductCommand command) =>
        $"Failed to update Product\nCommand: '{JsonConvert.SerializeObject(command)}'";
}