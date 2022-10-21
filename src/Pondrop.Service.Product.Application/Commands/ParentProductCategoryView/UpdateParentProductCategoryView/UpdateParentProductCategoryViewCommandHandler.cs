using AutoMapper;
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

namespace Pondrop.Service.Product.Application.Commands;

public class UpdateParentProductCategoryViewCommandHandler : IRequestHandler<UpdateParentProductCategoryViewCommand, Result<int>>
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

    public async Task<Result<int>> Handle(UpdateParentProductCategoryViewCommand command, CancellationToken cancellationToken)
    {
        if (!command.CategoryGroupingId.HasValue && !command.ProductCategoryId.HasValue)
            return Result<int>.Success(0);

        var result = default(Result<int>);

        try
        {
            var categoryGroupingsTask = _categoryGroupingContainerRepository.GetAllAsync();
            var productWithCategoryTask = _productWithCategoryContainerRepository.GetAllAsync();

            await Task.WhenAll(categoryGroupingsTask, productWithCategoryTask);

            var categoryGroupings = categoryGroupingsTask.Result;

            var tasks = productWithCategoryTask.Result.Select(async i =>
            {
                var success = false;

                try
                {
                    var product = await _productCheckpointRepository.GetByIdAsync(i.Id);

                    Guid? parentCategoryId = null;
                    if (i.Categories != null && i.Categories.Count() > 0)
                    {

                        var parentCategory = categoryGroupings?.FirstOrDefault(c => c.LowerLevelCategoryId == i.Categories.FirstOrDefault()?.Id);
                        parentCategoryId = parentCategory?.HigherLevelCategoryId;
                    }

                    string? barcodeNumber = null;
                    if (product != null)
                    {
                        var barcode = await _barcodeCheckpointRepository.QueryAsync($"SELECT * FROM c where c.productId = '{product.Id}'");
                        barcodeNumber = barcode?.FirstOrDefault()?.BarcodeNumber;
                    }

                    var categoryNames = i.Categories != null && i.Categories.Count > 0 ? string.Join(',', i.Categories.Select(s => i.Name)) : string.Empty;

                    var parentProductCategoryView = new ParentProductCategoryViewRecord(i.Id, parentCategoryId, i.Name, barcodeNumber, categoryNames, i.Categories);

                    var result = await _containerRepository.UpsertAsync(parentProductCategoryView);

                    success = result != null;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Failed to update parentProductCategoryView for '{i.Id}'");
                }

                return success;
            }).ToList();

            await Task.WhenAll(tasks);

            result = Result<int>.Success(tasks.Count(t => t.Result));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to rebuild category view");
            result = Result<int>.Error(ex);
        }

        return result;
    }

}