using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using Pondrop.Service.Interfaces;
using Pondrop.Service.Interfaces.Services;
using Pondrop.Service.Product.Application.Models;
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

            var categories = categoriesTask.Result;
            var categoryGroupings = categoryGroupingsTask.Result
                .GroupBy(i => i.HigherLevelCategoryId)
                .ToDictionary(g => g.Key, g => g.ToList());

            var upsertTasks = categories.Select(async i =>
            {
                var success = false;

                try
                {
                    categoryGroupings.TryGetValue(i.Id, out var lowerCategories);

                    var productCount = 0;
                    if (lowerCategories?.Count > 0)
                    {
                        var ids = string.Join(", ", lowerCategories
                            .Select(i => i.LowerLevelCategoryId)
                            .Distinct()
                            .Select(i => $"'{i}'"));
                        
                        var countResult = await _productCategoryEntityCheckpointRepository.QueryAsync<int>(
                            $"SELECT VALUE COUNT(1) FROM c WHERE c.categoryId IN ({ids})");
                        
                        productCount = countResult.FirstOrDefault();
                    }

                    var parentProductCategoryView = new ParentCategoryViewRecord(i.Id, i.Name, productCount);
                    var resultEntity = await _containerRepository.UpsertAsync(parentProductCategoryView);
                    success = resultEntity is not null;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Failed to update parentProductCategoryView for '{i.Id}'");
                }

                return success;
            }).ToList();

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
}