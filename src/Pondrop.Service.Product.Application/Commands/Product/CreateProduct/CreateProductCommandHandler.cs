using AutoMapper;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Pondrop.Service.Product.Application.Interfaces;
using Pondrop.Service.Product.Application.Interfaces.Services;
using Pondrop.Service.Product.Application.Models;
using Pondrop.Service.Product.Domain.Models.Product;

namespace Pondrop.Service.Product.Application.Commands;

public class CreateProductCommandHandler : DirtyCommandHandler<ProductEntity, CreateProductCommand, Result<ProductRecord>>
{
    private readonly IEventRepository _eventRepository;
    private readonly IMapper _mapper;
    private readonly IUserService _userService;
    private readonly ICheckpointRepository<ProductEntity> _checkpointRepository;
    private readonly IValidator<CreateProductCommand> _validator;
    private readonly ILogger<CreateProductCommandHandler> _logger;

    public CreateProductCommandHandler(
        IOptions<ProductUpdateConfiguration> ProductUpdateConfig,
        IEventRepository eventRepository,
        ICheckpointRepository<ProductEntity> checkpointRepository,
        IDaprService daprService,
        IUserService userService,
        IMapper mapper,
        IValidator<CreateProductCommand> validator,
        ILogger<CreateProductCommandHandler> logger) : base(eventRepository, ProductUpdateConfig.Value, daprService, logger)
    {
        _eventRepository = eventRepository;
        _mapper = mapper;
        _userService = userService;
        _checkpointRepository = checkpointRepository;
        _validator = validator;
        _logger = logger;
    }

    public override async Task<Result<ProductRecord>> Handle(CreateProductCommand command, CancellationToken cancellationToken)
    {
        var validation = _validator.Validate(command);

        if (!validation.IsValid)
        {
            var errorMessage = $"Create Product failed, errors on validation {validation}";
            _logger.LogError(errorMessage);
            return Result<ProductRecord>.Error(errorMessage);
        }

        var result = default(Result<ProductRecord>);

        try
        {

            var duplicateMessage = $"Possible product match found";
            var existingProduct = await GetExistingProductByName(command.Name);
            if (existingProduct != null && existingProduct.Count > 0)
                return Result<ProductRecord>.Error(duplicateMessage);

            var ProductEntity = new ProductEntity(
                command.Name,
                command.BrandId ?? Guid.Empty,
                command.ExternalReferenceId,
                command.Variant,
                command.AltName,
                command.ShortDescription,
                command.NetContent,
                command.NetContentUom,
                command.PossibleCategories,
                command.PublicationLifecycleId,
                command.ChildProductId ?? new List<Guid>(),
                _userService.CurrentUserId());

            var success = await _eventRepository.AppendEventsAsync(ProductEntity.StreamId, 0, ProductEntity.GetEvents());

            await Task.WhenAll(
                InvokeDaprMethods(ProductEntity.Id, ProductEntity.GetEvents()));

            result = success
                ? Result<ProductRecord>.Success(_mapper.Map<ProductRecord>(ProductEntity))
                : Result<ProductRecord>.Error(FailedToCreateMessage(command));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, FailedToCreateMessage(command));
            result = Result<ProductRecord>.Error(ex);
        }

        return result;
    }

    private async Task<List<ProductEntity>> GetExistingProductByName(string categoryName)
    {
        const string categoryNameKey = "@categoryName";

        var conditions = new List<string>();
        var parameters = new Dictionary<string, string>();

        conditions.Add($"LOWER(c.name) = {categoryNameKey}");
        parameters.Add(categoryNameKey, categoryName.ToLower());

        if (!conditions.Any())
            return new List<ProductEntity>(0);

        var sqlQueryText = $"SELECT * FROM c WHERE {string.Join(" AND ", conditions)}";

        var affectedProductCategories = await _checkpointRepository.QueryAsync(sqlQueryText, parameters);
        return affectedProductCategories;
    }


    private static string FailedToCreateMessage(CreateProductCommand command) =>
        $"Failed to create Product\nCommand: '{JsonConvert.SerializeObject(command)}'";
}