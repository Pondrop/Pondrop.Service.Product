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

public class SetProductsCommandHandler : DirtyCommandHandler<ProductCategoryEntity, SetProductsCommand, Result<List<ProductCategoryRecord>>>
{
    private readonly IEventRepository _eventRepository;
    private readonly ICheckpointRepository<ProductCategoryEntity> _ProductCategoryCheckpointRepository;
    private readonly IMapper _mapper;
    private readonly IUserService _userService;
    private readonly IValidator<SetProductsCommand> _validator;
    private readonly ILogger<SetProductsCommandHandler> _logger;

    public SetProductsCommandHandler(
        IOptions<ProductCategoryUpdateConfiguration> ProductCategoryUpdateConfig,
        IEventRepository eventRepository,
        ICheckpointRepository<ProductCategoryEntity> ProductCategoryCheckpointRepository,
        IDaprService daprService,
        IUserService userService,
        IMapper mapper,
        IValidator<SetProductsCommand> validator,
        ILogger<SetProductsCommandHandler> logger) : base(eventRepository, ProductCategoryUpdateConfig.Value, daprService, logger)
    {
        _eventRepository = eventRepository;
        _ProductCategoryCheckpointRepository = ProductCategoryCheckpointRepository;
        _mapper = mapper;
        _userService = userService;
        _validator = validator;
        _logger = logger;
    }

    public override async Task<Result<List<ProductCategoryRecord>>> Handle(SetProductsCommand command, CancellationToken cancellationToken)
    {
        var validation = _validator.Validate(command);

        if (!validation.IsValid)
        {
            var errorMessage = $"Update ProductCategory failed, errors on validation {validation}";
            _logger.LogError(errorMessage);
            return Result<List<ProductCategoryRecord>>.Error(errorMessage);
        }

        var result = default(Result<List<ProductCategoryRecord>>);

        try
        {
            var success = false;
            var results = new List<ProductCategoryRecord>();

            var productCategoryEntities = await _ProductCategoryCheckpointRepository.QueryAsync($"SELECT * FROM c WHERE c.deletedUtc = null AND c.categoryId = '{command.CategoryId}'");

            if (productCategoryEntities is not null)
            {
                foreach (var productCategoryEntity in productCategoryEntities)
                {
                    var evtPayload = new DeleteProductCategory(productCategoryEntity.Id);
                    var createdBy = _userService.CurrentUserId();

                    await UpdateStreamAsync(productCategoryEntity, evtPayload, createdBy);
                    await _ProductCategoryCheckpointRepository.FastForwardAsync(productCategoryEntity);

                    await Task.WhenAll(
                        InvokeDaprMethods(productCategoryEntity.Id, productCategoryEntity.GetEvents(productCategoryEntity.AtSequence)));

                    results.Add(_mapper.Map<ProductCategoryRecord>(productCategoryEntity));
                }

                if (command.ProductIds is not null)
                {
                    foreach (var productId in command.ProductIds)
                    {
                        var productCategoryEntity = new ProductCategoryEntity(
                            command.CategoryId,
                            productId,
                            command.PublicationLifecycleId,
                            _userService.CurrentUserId());

                        success = await _eventRepository.AppendEventsAsync(productCategoryEntity.StreamId, 0, productCategoryEntity.GetEvents());

                        await Task.WhenAll(
                            InvokeDaprMethods(productCategoryEntity.Id, productCategoryEntity.GetEvents(productCategoryEntity.AtSequence)));

                        results.Add(_mapper.Map<ProductCategoryRecord>(productCategoryEntity));
                    }
                }

                success = true;

                result = success
             ? Result<List<ProductCategoryRecord>>.Success(results)
             : Result<List<ProductCategoryRecord>>.Error(FailedToCreateMessage(command));
            }
            else
            {
                result = Result<List<ProductCategoryRecord>>.Error($"ProductCategory does not exist '{command.CategoryId}'");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, FailedToCreateMessage(command));
            result = Result<List<ProductCategoryRecord>>.Error(ex);
        }

        return result;
    }

    private static string FailedToCreateMessage(SetProductsCommand command) =>
        $"Failed to update ProductCategory\nCommand: '{JsonConvert.SerializeObject(command)}'";
}