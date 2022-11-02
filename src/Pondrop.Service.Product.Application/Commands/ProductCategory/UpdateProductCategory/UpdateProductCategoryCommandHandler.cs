using AutoMapper;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Pondrop.Service.Product.Application.Commands;
using Pondrop.Service.Product.Application.Interfaces;
using Pondrop.Service.Product.Application.Interfaces.Services;
using Pondrop.Service.Product.Application.Models;
using Pondrop.Service.Product.Domain.Events.Product;
using Pondrop.Service.Product.Domain.Events.ProductCategory;
using Pondrop.Service.Product.Domain.Models;
using Pondrop.Service.ProductCategory.Domain.Models;

namespace Pondrop.Service.Product.Application.Commands;

public class UpdateProductCategoryCommandHandler : DirtyCommandHandler<ProductCategoryEntity, UpdateProductCategoryCommand, Result<ProductCategoryRecord>>
{
    private readonly IEventRepository _eventRepository;
    private readonly ICheckpointRepository<ProductCategoryEntity> _ProductCategoryCheckpointRepository;
    private readonly IMapper _mapper;
    private readonly IUserService _userService;
    private readonly IValidator<UpdateProductCategoryCommand> _validator;
    private readonly ILogger<UpdateProductCategoryCommandHandler> _logger;

    public UpdateProductCategoryCommandHandler(
        IOptions<ProductCategoryUpdateConfiguration> ProductCategoryUpdateConfig,
        IEventRepository eventRepository,
        ICheckpointRepository<ProductCategoryEntity> ProductCategoryCheckpointRepository,
        IDaprService daprService,
        IUserService userService,
        IMapper mapper,
        IValidator<UpdateProductCategoryCommand> validator,
        ILogger<UpdateProductCategoryCommandHandler> logger) : base(eventRepository, ProductCategoryUpdateConfig.Value, daprService, logger)
    {
        _eventRepository = eventRepository;
        _ProductCategoryCheckpointRepository = ProductCategoryCheckpointRepository;
        _mapper = mapper;
        _userService = userService;
        _validator = validator;
        _logger = logger;
    }

    public override async Task<Result<ProductCategoryRecord>> Handle(UpdateProductCategoryCommand command, CancellationToken cancellationToken)
    {
        var validation = _validator.Validate(command);

        if (!validation.IsValid)
        {
            var errorMessage = $"Update ProductCategory failed, errors on validation {validation}";
            _logger.LogError(errorMessage);
            return Result<ProductCategoryRecord>.Error(errorMessage);
        }

        var result = default(Result<ProductCategoryRecord>);

        try
        {
            var ProductCategoryEntity = await _ProductCategoryCheckpointRepository.GetByIdAsync(command.Id);
            ProductCategoryEntity ??= await GetFromStreamAsync(command.Id);

            if (ProductCategoryEntity is not null)
            {
                var evtPayload = new UpdateProductCategory(
                    command.PublicationLifecycleId);
                var createdBy = _userService.CurrentUserId();

                var success = await UpdateStreamAsync(ProductCategoryEntity, evtPayload, createdBy);

                if (!success)
                {
                    await _ProductCategoryCheckpointRepository.FastForwardAsync(ProductCategoryEntity);
                    success = await UpdateStreamAsync(ProductCategoryEntity, evtPayload, createdBy);
                }

                await Task.WhenAll(
                    InvokeDaprMethods(ProductCategoryEntity.Id, ProductCategoryEntity.GetEvents(ProductCategoryEntity.AtSequence)));

                result = success
                    ? Result<ProductCategoryRecord>.Success(_mapper.Map<ProductCategoryRecord>(ProductCategoryEntity))
                    : Result<ProductCategoryRecord>.Error(FailedToCreateMessage(command));
            }
            else
            {
                result = Result<ProductCategoryRecord>.Error($"ProductCategory does not exist '{command.Id}'");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, FailedToCreateMessage(command));
            result = Result<ProductCategoryRecord>.Error(ex);
        }

        return result;
    }

    private static string FailedToCreateMessage(UpdateProductCategoryCommand command) =>
        $"Failed to update ProductCategory\nCommand: '{JsonConvert.SerializeObject(command)}'";
}