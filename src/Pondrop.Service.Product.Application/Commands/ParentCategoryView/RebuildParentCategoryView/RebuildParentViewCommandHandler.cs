using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Pondrop.Service.Product.Application.Interfaces;
using Pondrop.Service.Product.Application.Interfaces.Services;
using Pondrop.Service.Product.Application.Models;
using Pondrop.Service.Product.Domain.Events.Category;
using Pondrop.Service.Product.Domain.Models;
using Pondrop.Service.Product.Domain.Models.ProductCategory;
using Pondrop.Service.ProductCategory.Domain.Models;

namespace Pondrop.Service.Product.Application.Commands;

public class RebuildParentCategoryViewCommandHandler : IRequestHandler<RebuildParentCategoryViewCommand, Result<int>>
{
    private readonly ICheckpointRepository<CategoryGroupingEntity> _categoryGroupingCheckpointRepository;
    private readonly ICheckpointRepository<CategoryEntity> _categoryCheckpointRepository;
    private readonly ICheckpointRepository<ProductCategoryEntity> _productCategoryEntityCheckpointRepository;
    private readonly IContainerRepository<ParentCategoryViewRecord> _containerRepository;
    private readonly IMapper _mapper;
    private readonly IUserService _userService;
    private readonly ILogger<RebuildParentCategoryViewCommandHandler> _logger;

    public RebuildParentCategoryViewCommandHandler(
        ICheckpointRepository<CategoryGroupingEntity> categoryGroupingCheckpointRepository,
        ICheckpointRepository<ProductCategoryEntity> productCategoryEntityCheckpointRepository,
        ICheckpointRepository<CategoryEntity> categoryCheckpointRepository,
        IContainerRepository<ParentCategoryViewRecord> containerRepository,
        IMapper mapper,
        IUserService userService,
        ILogger<RebuildParentCategoryViewCommandHandler> logger) : base()
    {
        _categoryGroupingCheckpointRepository = categoryGroupingCheckpointRepository;
        _categoryCheckpointRepository = categoryCheckpointRepository;
        _productCategoryEntityCheckpointRepository = productCategoryEntityCheckpointRepository;
        _containerRepository = containerRepository;
        _mapper = mapper;
        _userService = userService;
        _logger = logger;
    }

    public async Task<Result<int>> Handle(RebuildParentCategoryViewCommand command, CancellationToken cancellationToken)
    {
        var result = default(Result<int>);

        try
        {
            var categoriesTask = _categoryCheckpointRepository.QueryAsync("SELECT * FROM c WHERE c.type = 'parent'");
            var categoryGroupingsTask = _categoryGroupingCheckpointRepository.GetAllAsync();

            await Task.WhenAll(categoryGroupingsTask, categoriesTask);

            var categoryGroupings = categoryGroupingsTask.Result;

            var tasks = categoriesTask.Result.Select(async i =>
            {
                var success = false;

                try
                {
                    var lowerCategories = categoryGroupings?.Where(c => c.HigherLevelCategoryId == i.Id);

                    var productCount = 0;
                    if (lowerCategories != null)
                    {
                        foreach (var category in lowerCategories)
                        {
                            var productCategories = await _productCategoryEntityCheckpointRepository.QueryAsync($"SELECT * FROM c WHERE c.categoryId = '{category.LowerLevelCategoryId}'");
                            productCount += productCategories?.Count() ?? 0;
                        }
                    }

                    var parentProductCategoryView = new ParentCategoryViewRecord(i.Id, i.Name, productCount);

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