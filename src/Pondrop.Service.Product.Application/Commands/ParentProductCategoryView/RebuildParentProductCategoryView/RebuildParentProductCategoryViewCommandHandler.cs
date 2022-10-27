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

public class RebuildParentProductCategoryViewCommandHandler : IRequestHandler<RebuildParentProductCategoryViewCommand, Result<int>>
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
    private readonly ILogger<RebuildParentProductCategoryViewCommandHandler> _logger;

    public RebuildParentProductCategoryViewCommandHandler(
        IContainerRepository<CategoryGroupingViewRecord> categoryGroupingContainerRepository,
        IContainerRepository<ProductWithCategoryViewRecord> productWithCategoryContainerRepository,
        ICheckpointRepository<ProductCategoryEntity> productCategoryCheckpointRepository,
        ICheckpointRepository<BarcodeEntity> barcodeCheckpointRepository,
        ICheckpointRepository<ProductEntity> productCheckpointRepository,
        ICheckpointRepository<CategoryEntity> categoryCheckpointRepository,
        IContainerRepository<ParentProductCategoryViewRecord> containerRepository,
        IMapper mapper,
        IUserService userService,
        ILogger<RebuildParentProductCategoryViewCommandHandler> logger) : base()
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

    public async Task<Result<int>> Handle(RebuildParentProductCategoryViewCommand command, CancellationToken cancellationToken)
    {
        var result = default(Result<int>);

        try
        {
            var statusMsgs = new List<String>();
            var sw = new Stopwatch();
            sw.Start();
            
            var categoryGroupingsTask = _categoryGroupingContainerRepository.GetAllAsync();
            var productWithCategoryTask = _productWithCategoryContainerRepository.GetAllAsync();
            var barcodesTask = _barcodeCheckpointRepository.GetAllAsync();

            await Task.WhenAll(categoryGroupingsTask, productWithCategoryTask, barcodesTask);

           var categoryLowerLookup = categoryGroupingsTask.Result
               .GroupBy(i => i.LowerLevelCategoryId)
               .ToDictionary(g => g.Key, i => i.First().HigherLevelCategoryId);
           var productWithCategories = productWithCategoryTask.Result;
           
           var barcodeLookup = barcodesTask.Result
               .GroupBy(i => i.ProductId)
               .ToDictionary(g => g.Key, g => g.First());

           statusMsgs.Add($"Got required data: {sw.Elapsed.TotalSeconds / 60}mins");
           
           var upsertTasks = new List<Task<bool>>();

           for (var i = 0; i < productWithCategories.Count; i++)
           {
               System.Diagnostics.Debug.WriteLine($"# Starting ParentProductCategoryView task {i + 1} of {productWithCategories.Count}");
               var product = productWithCategories[i];
               
               var task = Task.Run(async () =>
               {
                   var success = false;

                   try
                   {
                       Guid? parentCategoryId = null;
                       if (product.Categories?.Count > 0)
                       {
                           categoryLowerLookup.TryGetValue(product.Categories.FirstOrDefault()?.Id ?? Guid.Empty, out var higherLevelCategoryId);
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
                   System.Diagnostics.Debug.WriteLine($"## Waiting for previous ParentProductCategoryView tasks {upsertTasks.Count} of {productWithCategories.Count}");
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