using AutoMapper;
using Google.Type;
using MediatR;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Pondrop.Service.Product.Application.Interfaces;
using Pondrop.Service.Product.Application.Interfaces.Services;
using Pondrop.Service.Product.Application.Models;
using Pondrop.Service.Product.Domain.Events.Category;
using Pondrop.Service.Product.Domain.Models;
using Pondrop.Service.Product.Domain.Models.Product;
using Pondrop.Service.Product.Domain.Models.ProductCategory;
using Pondrop.Service.ProductCategory.Domain.Models;
using System.Diagnostics;
using DateTime = System.DateTime;

namespace Pondrop.Service.Product.Application.Commands;

public class RebuildProductViewCommandHandler : IRequestHandler<RebuildProductViewCommand, Result<int>>
{
    private readonly IContainerRepository<CategoryGroupingViewRecord> _categoryGroupingContainerRepository;
    private readonly ICheckpointRepository<CategoryEntity> _categoryCheckpointRepository;
    private readonly ICheckpointRepository<BarcodeEntity> _barcodeCheckpointRepository;
    private readonly ICheckpointRepository<ProductEntity> _productCheckpointRepository;
    private readonly ICheckpointRepository<ProductCategoryEntity> _productCategoryCheckpointRepository;
    private readonly IContainerRepository<ProductViewRecord> _containerRepository;
    private readonly IMapper _mapper;
    private readonly IUserService _userService;
    private readonly ILogger<RebuildProductViewCommandHandler> _logger;

    public RebuildProductViewCommandHandler(
        IContainerRepository<CategoryGroupingViewRecord> categoryGroupingContainerRepository,
        ICheckpointRepository<ProductCategoryEntity> productCategoryCheckpointRepository,
        ICheckpointRepository<BarcodeEntity> barcodeCheckpointRepository,
        ICheckpointRepository<ProductEntity> productCheckpointRepository,
        ICheckpointRepository<CategoryEntity> categoryCheckpointRepository,
        IContainerRepository<ProductViewRecord> containerRepository,
        IMapper mapper,
        IUserService userService,
        ILogger<RebuildProductViewCommandHandler> logger) : base()
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

    public async Task<Result<int>> Handle(RebuildProductViewCommand command, CancellationToken cancellationToken)
    {
        var result = default(Result<int>);

        try
        {
            var statusMsgs = new List<String>();
            var sw = new Stopwatch();
            sw.Start();

            var productsTask = _productCheckpointRepository.GetAllAsync();
            var categoryGroupingsTask = _categoryGroupingContainerRepository.GetAllAsync();
            var categoryTask = _categoryCheckpointRepository.GetAllAsync();
            var productCategoryTask = _productCategoryCheckpointRepository.GetAllAsync();
            var barcodesTask = _barcodeCheckpointRepository.GetAllAsync();

            await Task.WhenAll(productsTask, categoryGroupingsTask, categoryTask, productCategoryTask, barcodesTask);

            var products = productsTask.Result;

            var categoryLowerLookup = categoryGroupingsTask.Result
                .GroupBy(i => i.LowerLevelCategoryId)
                .ToDictionary(g => g.Key, i => i.First().HigherLevelCategoryId);

            var categoryLookup = categoryTask.Result
                .GroupBy(i => i.Id)
                .ToDictionary(g => g.Key, i => i.First());

            var productCategoryLookup = productCategoryTask?.Result
                  .Where(p => !p.DeletedUtc.HasValue)
                  .GroupBy(i => i.ProductId)
                  .ToDictionary(g => g.Key, i => new List<Guid>(i.Select(s => s.CategoryId)));

            var barcodeLookup = barcodesTask.Result
                .GroupBy(i => i.ProductId)
                .ToDictionary(g => g.Key, g => g.First());

            statusMsgs.Add($"Got required data: {sw.Elapsed.TotalSeconds / 60}mins");

            var upsertTasks = new List<Task<bool>>();

            for (var i = 0; i < products.Count; i++)
            {
                System.Diagnostics.Debug.WriteLine($"# Starting ParentProductCategoryView task {i + 1} of {products.Count}");


                var task = Task.Run(async () =>
                {
                    var success = false;

                    try
                    {
                        var product = products[i];
                        var categoryIds = new List<Guid>();
                        List<CategoryEntity>? categories = new List<CategoryEntity>();

                        productCategoryLookup?.TryGetValue(product.Id, out categoryIds);


                        foreach (var categoryId in categoryIds)
                        {
                            CategoryEntity? category = null;
                            categoryLookup?.TryGetValue(categoryId, out category);
                            categories.Add(category);
                        }


                        Guid? parentCategoryId = null;

                        if (categories != null && categories.Count > 0)
                        {
                            var higherLevelCategoryId = Guid.Empty;

                            categoryLowerLookup?.TryGetValue(categories.FirstOrDefault()?.Id ?? Guid.Empty, out higherLevelCategoryId);
                            parentCategoryId = higherLevelCategoryId;
                        }

                        categoryLookup.TryGetValue(parentCategoryId ?? Guid.Empty, out var parentCategory);

                        barcodeLookup.TryGetValue(product.Id, out var barcodes);
                        var barcodeNumber = barcodes?.BarcodeNumber;


                        var categoryNames = categories is not null && categories.Count > 0
                            ? String.Join(',', categories.Select(s => s.Name))
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
                          categories != null && categories.Count > 0 ? _mapper.Map<List<CategoryViewRecord>>(categories) : null);

                        var upsertEntity = await _containerRepository.UpsertAsync(productView);
                        success = upsertEntity is not null;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Failed to update parentProductCategoryView for '{products[i].Id}'");
                    }

                    return success;
                }, cancellationToken);

                upsertTasks.Add(task);

                const int batchSize = 256;
                if (upsertTasks.Count % batchSize == 0)
                {
                    //System.Diagnostics.Debug.WriteLine($"## Waiting for previous ParentProductCategoryView tasks {upsertTasks.Count} of {productWithCategories.Count}");
                    await Task.WhenAll(upsertTasks.TakeLast(batchSize));
                }
            }

            await Task.WhenAll(upsertTasks);
            result = Result<int>.Success(upsertTasks.Count(t => t.Result));

            sw.Stop();
            statusMsgs.Add($"Finished: {sw.Elapsed.TotalSeconds / 60d}mins");

            foreach (var i in statusMsgs)
            {
                System.Diagnostics.Debug.WriteLine(i);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to rebuild category view");
            result = Result<int>.Error(ex);
        }

        return result;
    }
}