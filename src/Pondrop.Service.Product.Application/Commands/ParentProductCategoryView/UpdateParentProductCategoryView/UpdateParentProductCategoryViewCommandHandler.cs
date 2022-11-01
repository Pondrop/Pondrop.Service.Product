using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using Pondrop.Service.Product.Application.Interfaces;
using Pondrop.Service.Product.Application.Interfaces.Services;
using Pondrop.Service.Product.Application.Models;
using Pondrop.Service.Product.Domain.Models;
using Pondrop.Service.Product.Domain.Models.Product;
using Pondrop.Service.Product.Domain.Models.ProductCategory;
using Pondrop.Service.ProductCategory.Domain.Models;

namespace Pondrop.Service.Product.Application.Commands;

public class
    UpdateParentProductCategoryViewCommandHandler : IRequestHandler<UpdateParentProductCategoryViewCommand, Result<int>>
{
    private readonly IContainerRepository<CategoryGroupingViewRecord> _categoryGroupingContainerRepository;
    private readonly ICheckpointRepository<CategoryEntity> _categoryCheckpointRepository;
    private readonly IContainerRepository<ProductWithCategoryViewRecord> _productWithCategoryContainerRepository;
    private readonly ICheckpointRepository<BarcodeEntity> _barcodeCheckpointRepository;
    private readonly ICheckpointRepository<ProductEntity> _productCheckpointRepository;
    private readonly ICheckpointRepository<ProductCategoryEntity> _productCategoryCheckpointRepository;
    private readonly IContainerRepository<ParentProductCategoryViewRecord> _containerRepository;
    private readonly IMapper _mapper;
    private readonly IUserService _userService;
    private readonly ILogger<UpdateParentProductCategoryViewCommandHandler> _logger;

    public UpdateParentProductCategoryViewCommandHandler(
        IContainerRepository<CategoryGroupingViewRecord> categoryGroupingContainerRepository,
        IContainerRepository<ProductWithCategoryViewRecord> productWithCategoryContainerRepository,
        ICheckpointRepository<ProductCategoryEntity> productCategoryCheckpointRepository,
        ICheckpointRepository<BarcodeEntity> barcodeCheckpointRepository,
        ICheckpointRepository<ProductEntity> productCheckpointRepository,
        ICheckpointRepository<CategoryEntity> categoryCheckpointRepository,
        IContainerRepository<ParentProductCategoryViewRecord> containerRepository,
        IMapper mapper,
        IUserService userService,
        ILogger<UpdateParentProductCategoryViewCommandHandler> logger) : base()
    {
        _categoryGroupingContainerRepository = categoryGroupingContainerRepository;
        _categoryCheckpointRepository = categoryCheckpointRepository;
        _productCheckpointRepository = productCheckpointRepository;
        _productCategoryCheckpointRepository = productCategoryCheckpointRepository;
        _productWithCategoryContainerRepository = productWithCategoryContainerRepository;
        _barcodeCheckpointRepository = barcodeCheckpointRepository;
        _containerRepository = containerRepository;
        _mapper = mapper;
        _userService = userService;
        _logger = logger;
    }

    public async Task<Result<int>> Handle(UpdateParentProductCategoryViewCommand command,
        CancellationToken cancellationToken)
    {
        if (!command.CategoryId.HasValue && !command.ProductId.HasValue)
            return Result<int>.Success(0);

        var result = default(Result<int>);

        try
        {
            var affectedItems = await GetAffectedItemsAsync(command.CategoryId, command.ProductId);

            var lowerCatIdString = string.Join(", ", affectedItems
                .Where(i => i.Categories?.Any() == true)
                .Select(i => $"'{i.Categories!.First()}'"));
            var categoryGroupingsTask =
                _categoryGroupingContainerRepository.QueryAsync(
                    $"SELECT * FROM c WHERE c.lowerLevelCategoryId IN ({lowerCatIdString})");

            var productWithCategoryTask =
                _productWithCategoryContainerRepository.GetByIdsAsync(affectedItems.Select(i => i.Id));

            var productIdString = string.Join(", ", affectedItems.Select(i => $"'{i.Id}'"));
            var barcodesTask =
                _barcodeCheckpointRepository.QueryAsync($"SELECT * FROM c WHERE c.productId IN ({productIdString})");

            await Task.WhenAll(categoryGroupingsTask, productWithCategoryTask, barcodesTask);

            var categoryLowerLookup = categoryGroupingsTask.Result
                .GroupBy(i => i.LowerLevelCategoryId)
                .ToDictionary(g => g.Key, i => i.First().HigherLevelCategoryId);
            var productWithCategories = productWithCategoryTask.Result;

            var barcodeLookup = barcodesTask.Result
                .GroupBy(i => i.ProductId)
                .ToDictionary(g => g.Key, g => g.First());

            var upsertTasks = new List<Task<bool>>();

            for (var i = 0; i < productWithCategories.Count; i++)
            {
                var product = productWithCategories[i];

                var task = Task.Run(async () =>
                {
                    var success = false;

                    try
                    {
                        Guid? parentCategoryId = null;
                        if (product.Categories?.Count > 0)
                        {
                            categoryLowerLookup.TryGetValue(product.Categories.FirstOrDefault()?.Id ?? Guid.Empty,
                                out var higherLevelCategoryId);
                            parentCategoryId = higherLevelCategoryId;
                        }

                        barcodeLookup.TryGetValue(product.Id, out var barcodes);
                        var barcodeNumber = barcodes?.BarcodeNumber;

                        var categoryNames = product.Categories?.Count > 0
                            ? string.Join(',', product.Categories.Select(s => s.Name))
                            : string.Empty;

                        var parentProductCategoryView = new ParentProductCategoryViewRecord(
                            product.Id,
                            parentCategoryId,
                            product.Name,
                            barcodeNumber,
                            categoryNames,
                            product.Categories);

                        var upsertEntity = await _containerRepository.UpsertAsync(parentProductCategoryView);
                        success = upsertEntity is not null;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Failed to update parentProductCategoryView for '{product.Id}'");
                    }

                    return success;
                }, cancellationToken);

                upsertTasks.Add(task);

                const int batchSize = 256;
                if (upsertTasks.Count % batchSize == 0)
                {
                    System.Diagnostics.Debug.WriteLine(
                        $"## Waiting for previous ParentProductCategoryView tasks {upsertTasks.Count} of {productWithCategories.Count}");
                    await Task.WhenAll(upsertTasks.TakeLast(batchSize));
                }
            }

            await Task.WhenAll(upsertTasks);
            result = Result<int>.Success(upsertTasks.Count(t => t.Result));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to rebuild category view");
            result = Result<int>.Error(ex);
        }

        return result;
    }


    private async Task<List<ParentProductCategoryViewRecord>> GetAffectedItemsAsync(Guid? categoryId,
        Guid? productId)
    {
        const string categoryIdKey = "@categoryId";
        const string productIdKey = "@productId";

        var conditions = new List<string>();
        var parameters = new Dictionary<string, string>();

        if (categoryId.HasValue)
        {
            conditions.Add($"(c.id = {categoryId})");
            parameters.Add(categoryIdKey, categoryId.Value.ToString());
        }

        if (productId.HasValue)
        {
            conditions.Add($"p.id = {productIdKey}");
            parameters.Add(productIdKey, productId.Value.ToString());
        }

        if (!conditions.Any())
            return new List<ParentProductCategoryViewRecord>(0);

        var sqlQueryText = categoryId.HasValue
            ? $"SELECT VALUE p FROM p JOIN c IN p.categories WHERE {string.Join(" AND ", conditions)}"
            : $"SELECT * FROM p WHERE {string.Join(" AND ", conditions)}";

        var affected = await _containerRepository.QueryAsync(sqlQueryText, parameters);
        return affected;
    }
}