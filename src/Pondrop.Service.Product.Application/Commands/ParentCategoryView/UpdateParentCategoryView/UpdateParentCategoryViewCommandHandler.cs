using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Pondrop.Service.Interfaces;
using Pondrop.Service.Interfaces.Services;
using Pondrop.Service.Product.Application.Models;
using Pondrop.Service.Product.Domain.Models;
using Pondrop.Service.Product.Domain.Models.ProductCategory;
using Pondrop.Service.ProductCategory.Domain.Models;

namespace Pondrop.Service.Product.Application.Commands;

public class UpdateParentCategoryViewCommandHandler : IRequestHandler<UpdateParentCategoryViewCommand, Result<int>>
{
    private readonly ICheckpointRepository<CategoryGroupingEntity> _categoryGroupingCheckpointRepository;
    private readonly ICheckpointRepository<CategoryEntity> _categoryCheckpointRepository;
    private readonly ICheckpointRepository<ProductCategoryEntity> _productCategoryCheckpointRepository;
    private readonly IContainerRepository<ParentCategoryViewRecord> _containerRepository;
    private readonly IMapper _mapper;
    private readonly IUserService _userService;
    private readonly ILogger<UpdateParentCategoryViewCommandHandler> _logger;

    public UpdateParentCategoryViewCommandHandler(
        ICheckpointRepository<CategoryGroupingEntity> categoryGroupingCheckpointRepository,
        ICheckpointRepository<CategoryEntity> categoryCheckpointRepository,
        ICheckpointRepository<ProductCategoryEntity> productCategoryCheckpointRepository,
    IContainerRepository<ParentCategoryViewRecord> containerRepository,
        IMapper mapper,
        IUserService userService,
        ILogger<UpdateParentCategoryViewCommandHandler> logger) : base()
    {
        _categoryGroupingCheckpointRepository = categoryGroupingCheckpointRepository;
        _categoryCheckpointRepository = categoryCheckpointRepository;
        _productCategoryCheckpointRepository = productCategoryCheckpointRepository;
        _containerRepository = containerRepository;
        _mapper = mapper;
        _userService = userService;
        _logger = logger;
    }

    public async Task<Result<int>> Handle(UpdateParentCategoryViewCommand command, CancellationToken cancellationToken)
    {
        if (!command.CategoryId.HasValue && !command.ProductId.HasValue)
            return Result<int>.Success(0);

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
                            var productCategories = await _productCategoryCheckpointRepository.QueryAsync($"SELECT * FROM c WHERE c.categoryId = '{category.LowerLevelCategoryId}'");
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
            _logger.LogError(ex, FailedToMessage(command));
            result = Result<int>.Error(ex);
        }

        return result;
    }


    private async Task<List<CategoryGroupingEntity>> GetAffectedCategoryGroupingAsync(Guid? categoryGroupingId, Guid? categoryId)
    {
        const string categoryIdKey = "@categoryId";
        const string categoryGroupingIdKey = "@categoryGroupingId";

        var conditions = new List<string>();
        var parameters = new Dictionary<string, string>();

        if (categoryGroupingId.HasValue)
        {
            conditions.Add($"c.id = {categoryGroupingIdKey}");
            parameters.Add(categoryGroupingIdKey, categoryGroupingId.Value.ToString());
        }
        if (categoryId.HasValue)
        {
            conditions.Add($"(c.higherLevelCategoryId = {categoryIdKey} OR c.lowerLevelCategoryId = {categoryIdKey})");
            parameters.Add(categoryIdKey, categoryId.Value.ToString());
        }

        if (!conditions.Any())
            return new List<CategoryGroupingEntity>(0);

        var sqlQueryText = $"SELECT * FROM c WHERE {string.Join(" AND ", conditions)}";

        var affectedStores = await _categoryGroupingCheckpointRepository.QueryAsync(sqlQueryText, parameters);
        return affectedStores;
    }

    private async Task<List<ProductCategoryEntity>> GetAffectedProducttCategoryAsync(Guid? productCategoryId)
    {
        const string productCategoryIdKey = "@productCategoryId";

        var conditions = new List<string>();
        var parameters = new Dictionary<string, string>();

        if (productCategoryId.HasValue)
        {
            conditions.Add($"c.id = {productCategoryIdKey}");
            parameters.Add(productCategoryIdKey, productCategoryId.Value.ToString());
        }

        if (!conditions.Any())
            return new List<ProductCategoryEntity>(0);

        var sqlQueryText = $"SELECT * FROM c WHERE {string.Join(" AND ", conditions)}";

        var affectedStores = await _productCategoryCheckpointRepository.QueryAsync(sqlQueryText, parameters);
        return affectedStores;
    }

    private static string FailedToMessage(UpdateParentCategoryViewCommand command) =>
        $"Failed to update category view '{JsonConvert.SerializeObject(command)}'";
}