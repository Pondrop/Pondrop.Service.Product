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
    UpdateParentProductCategoryViewCommandHandler : IRequestHandler<UpdateProductViewCommand, Result<int>>
{
    private readonly IContainerRepository<CategoryGroupingViewRecord> _categoryGroupingContainerRepository;
    private readonly ICheckpointRepository<CategoryEntity> _categoryCheckpointRepository;
    private readonly ICheckpointRepository<BarcodeEntity> _barcodeCheckpointRepository;
    private readonly ICheckpointRepository<ProductEntity> _productCheckpointRepository;
    private readonly ICheckpointRepository<ProductCategoryEntity> _productCategoryCheckpointRepository;
    private readonly IContainerRepository<ProductViewRecord> _containerRepository;
    private readonly IMapper _mapper;
    private readonly IUserService _userService;
    private readonly ILogger<UpdateParentProductCategoryViewCommandHandler> _logger;

    public UpdateParentProductCategoryViewCommandHandler(
        IContainerRepository<CategoryGroupingViewRecord> categoryGroupingContainerRepository,
        ICheckpointRepository<ProductCategoryEntity> productCategoryCheckpointRepository,
        ICheckpointRepository<BarcodeEntity> barcodeCheckpointRepository,
        ICheckpointRepository<ProductEntity> productCheckpointRepository,
        ICheckpointRepository<CategoryEntity> categoryCheckpointRepository,
        IContainerRepository<ProductViewRecord> containerRepository,
        IMapper mapper,
        IUserService userService,
        ILogger<UpdateParentProductCategoryViewCommandHandler> logger) : base()
    {
        _categoryGroupingContainerRepository = categoryGroupingContainerRepository;
        _categoryCheckpointRepository = categoryCheckpointRepository;
        _productCheckpointRepository = productCheckpointRepository;
        _productCategoryCheckpointRepository = productCategoryCheckpointRepository;
        _barcodeCheckpointRepository = barcodeCheckpointRepository;
        _containerRepository = containerRepository;
        _mapper = mapper;
        _userService = userService;
        _logger = logger;
    }

    public async Task<Result<int>> Handle(UpdateProductViewCommand command,
        CancellationToken cancellationToken)
    {
        if (!command.CategoryId.HasValue && !command.ProductId.HasValue)
            return Result<int>.Success(0);

        var result = default(Result<int>);

        try
        {
            var affectedItems = await GetAffectedItemsAsync(command.CategoryId, command.ProductId);

            var productIds = affectedItems.Select(i => i.Id).ToHashSet();
            var categoryIds = affectedItems.Select(i => i.Categories?.FirstOrDefault()?.Id).ToHashSet();

            if (command.ProductId.HasValue)
            {
                productIds.Add(command.ProductId.Value);
            }

            if (command.CategoryId.HasValue)
            {
                categoryIds.Add(command.CategoryId.Value);
            }

            var lowerCatIdString = string.Join(", ", categoryIds.Select(i => $"'{i}'"));

            var categoryGroupingsTask = string.IsNullOrEmpty(lowerCatIdString) ? null :
                _categoryGroupingContainerRepository.QueryAsync(
                    $"SELECT * FROM c WHERE c.lowerLevelCategoryId IN ({lowerCatIdString})");

            var productIdString = string.Join(", ", productIds.Select(i => $"'{i}'"));

            var categoryTask = string.IsNullOrEmpty(lowerCatIdString) ? null :
                _categoryCheckpointRepository.QueryAsync(
                    $"SELECT * FROM c WHERE c.id IN ({lowerCatIdString})");

            var conditions = new List<string>();

            conditions.Add(!string.IsNullOrEmpty(lowerCatIdString) ? $"c.categoryId IN ({lowerCatIdString})" : string.Empty);
            conditions.Add(!string.IsNullOrEmpty(productIdString) ? $"c.productId IN ({productIdString})" : string.Empty);

            var productCategoryTask = _productCategoryCheckpointRepository.QueryAsync(
                    $"SELECT * FROM c WHERE {string.Join(" OR ", conditions)}");

            var barcodesTask = string.IsNullOrEmpty(productIdString) ? null :
                _barcodeCheckpointRepository.QueryAsync($"SELECT * FROM c WHERE c.productId IN ({productIdString})");

            var productsTask = string.IsNullOrEmpty(productIdString) ? null :
                _productCheckpointRepository.QueryAsync($"SELECT * FROM c WHERE c.id IN ({productIdString})");

            await Task.WhenAll(new Task[] { categoryGroupingsTask, categoryTask, productCategoryTask, barcodesTask, productsTask }.Where(i => i != null));

            var products = productsTask.Result;

            var categoryLowerLookup = categoryGroupingsTask?.Result
                .GroupBy(i => i.LowerLevelCategoryId)
                .ToDictionary(g => g.Key, i => i.First().HigherLevelCategoryId);

            var categoryLookup = categoryTask?.Result
                .GroupBy(i => i.Id)
                .ToDictionary(g => g.Key, i => i.First());

            var productCategoryLookup = productCategoryTask?.Result
                .GroupBy(i => i.ProductId)
                .ToDictionary(g => g.Key, i => i.First().CategoryId);

            var barcodeLookup = barcodesTask?.Result
                .GroupBy(i => i.ProductId)
                .ToDictionary(g => g.Key, g => g.First());

            var upsertTasks = new List<Task<bool>>();

            for (var i = 0; i < products.Count; i++)
            {
                var product = products[i];

                var task = Task.Run(async () =>
                {
                    var success = false;

                    try
                    {
                        var categoryId = Guid.Empty;
                        CategoryEntity? category = null;

                        productCategoryLookup?.TryGetValue(product.Id, out categoryId);
                        categoryLookup?.TryGetValue(categoryId, out category);


                        Guid? parentCategoryId = null;
                        if (category != null)
                        {
                            var higherLevelCategoryId = Guid.Empty;
                            categoryLowerLookup?.TryGetValue(category?.Id ?? Guid.Empty, out higherLevelCategoryId);
                            parentCategoryId = higherLevelCategoryId;
                        }

                        CategoryEntity? parentCategory = null;
                        categoryLookup?.TryGetValue(parentCategoryId ?? Guid.Empty, out parentCategory);

                        BarcodeEntity? barcode = null;
                        barcodeLookup?.TryGetValue(product.Id, out barcode);
                        var barcodeNumber = barcode?.BarcodeNumber;

                        var categoryNames = category?.Id is not null
                            ? category.Name
                            : string.Empty;

                        var productView = new ProductViewRecord(
                            product.Id,
                            parentCategoryId,
                            product.Name,
                            product.BrandId,
                            product.ExternalReferenceId,
                            product.Variant,
                            product.AltName,
                            product.ShortDescription,
                            product.NetContent,
                            product.NetContentUom,
                            product.PossibleCategories,
                            product.PublicationLifecycleId,
                            product.ChildProductId,
                            barcodeNumber,
                            categoryNames,
                            parentCategory != null ? new CategoryViewRecord(parentCategory.Id, parentCategory.Name, parentCategory.Type) : null,
                            category != null ? new List<CategoryViewRecord>() { new CategoryViewRecord(category.Id, category.Name, category.Type) } : null);

                        var upsertEntity = await _containerRepository.UpsertAsync(productView);
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
                        $"## Waiting for previous ParentProductCategoryView tasks {upsertTasks.Count} of {products.Count}");
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


    private async Task<List<ProductViewRecord>> GetAffectedItemsAsync(Guid? categoryId,
        Guid? productId)
    {
        const string categoryIdKey = "@categoryId";
        const string productIdKey = "@productId";

        var conditions = new List<string>();
        var parameters = new Dictionary<string, string>();

        if (categoryId.HasValue)
        {
            conditions.Add($"(c.id = '{categoryId}')");
            parameters.Add(categoryIdKey, categoryId.Value.ToString());
        }

        if (productId.HasValue)
        {
            conditions.Add($"p.id = '{productIdKey}'");
            parameters.Add(productIdKey, productId.Value.ToString());
        }

        if (!conditions.Any())
            return new List<ProductViewRecord>(0);

        var sqlQueryText = categoryId.HasValue
            ? $"SELECT VALUE p FROM p JOIN c IN p.categories WHERE {string.Join(" AND ", conditions)}"
            : $"SELECT * FROM p WHERE {string.Join(" AND ", conditions)}";

        var affected = await _containerRepository.QueryAsync(sqlQueryText, parameters);
        return affected;
    }

    private async Task<List<ProductViewRecord>> GetProductCategoryItemsAsync(Guid? categoryId,
        Guid? productId)
    {
        const string categoryIdKey = "@categoryId";
        const string productIdKey = "@productId";

        var conditions = new List<string>();
        var parameters = new Dictionary<string, string>();

        if (categoryId.HasValue)
        {
            conditions.Add($"(c.categoryId = '{categoryId}')");
            parameters.Add(categoryIdKey, categoryId.Value.ToString());
        }

        if (productId.HasValue)
        {
            conditions.Add($"p.productId = '{productIdKey}'");
            parameters.Add(productIdKey, productId.Value.ToString());
        }

        if (!conditions.Any())
            return new List<ProductViewRecord>(0);

        var sqlQueryText = $"SELECT * FROM p WHERE {string.Join(" AND ", conditions)}";

        var affected = await _containerRepository.QueryAsync(sqlQueryText, parameters);
        return affected;
    }
}