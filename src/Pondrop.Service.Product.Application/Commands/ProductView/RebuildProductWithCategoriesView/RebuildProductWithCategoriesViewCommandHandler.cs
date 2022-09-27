using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Pondrop.Service.Product.Application.Interfaces;
using Pondrop.Service.Product.Application.Interfaces.Services;
using Pondrop.Service.Product.Application.Models;
using Pondrop.Service.Product.Domain.Events.Category;
using Pondrop.Service.Product.Domain.Models;
using Pondrop.Service.Product.Domain.Models.Category;
using Pondrop.Service.Product.Domain.Models.Product;
using Pondrop.Service.ProductCategory.Domain.Models;

namespace Pondrop.Service.Product.Application.Commands;

public class RebuildProductWithCategoriesViewCommandHandler : IRequestHandler<RebuildProductWithCategoryViewCommand, Result<int>>
{
    private readonly ICheckpointRepository<CategoryEntity> _categoryCheckpointRepository;
    private readonly ICheckpointRepository<ProductEntity> _productCheckpointRepository;
    private readonly ICheckpointRepository<ProductCategoryEntity> _productCategoryCheckpointRepository;
    private readonly IContainerRepository<ProductWithCategoryViewRecord> _containerRepository;
    private readonly IMapper _mapper;
    private readonly IUserService _userService;
    private readonly ILogger<RebuildProductWithCategoriesViewCommandHandler> _logger;

    public RebuildProductWithCategoriesViewCommandHandler(
        ICheckpointRepository<CategoryEntity> categoryCheckpointRepository,
        ICheckpointRepository<ProductEntity> productCheckpointRepository,
        ICheckpointRepository<ProductCategoryEntity> productCategoryCheckpointRepository,
        IContainerRepository<ProductWithCategoryViewRecord> containerRepository,
        IMapper mapper,
        IUserService userService,
        ILogger<RebuildProductWithCategoriesViewCommandHandler> logger) : base()
    {
        _categoryCheckpointRepository = categoryCheckpointRepository;
        _productCheckpointRepository = productCheckpointRepository;
        _productCategoryCheckpointRepository = productCategoryCheckpointRepository;
        _containerRepository = containerRepository;
        _mapper = mapper;
        _userService = userService;
        _logger = logger;
    }

    public async Task<Result<int>> Handle(RebuildProductWithCategoryViewCommand command, CancellationToken cancellationToken)
    {
        var result = default(Result<int>);

        try
        {
            var productCategories = _productCategoryCheckpointRepository.GetAllAsync();

            await Task.WhenAll(productCategories);

            var tasks = productCategories.Result.Select(async i =>
            {

                var productTask = _productCheckpointRepository.GetByIdAsync(i.ProductId);

                await Task.WhenAll(productTask);

                var success = false;

                var ProductWithCategoriesView = _mapper.Map<ProductWithCategoryViewRecord>(productTask.Result);

                try
                {
                    var affectedCategory = await _categoryCheckpointRepository.GetByIdAsync(i.CategoryId);

                    if (affectedCategory != null)
                        ProductWithCategoriesView.Categories.Add(_mapper.Map<CategoryViewRecord>(affectedCategory));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Failed to update category view for '{productTask.Result.Id}'");
                }

                var upsertResult = await _containerRepository.UpsertAsync(ProductWithCategoriesView);

                result = Result<int>.Success(1);

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

    private async Task<List<ProductCategoryEntity>> GetAffectedProductCategoriesAsync(Guid? categoryId)
    {
        const string categoryIdKey = "@categoryId";

        var conditions = new List<string>();
        var parameters = new Dictionary<string, string>();

        if (categoryId.HasValue)
        {
            conditions.Add($"c.categoryId = {categoryIdKey}");
            parameters.Add(categoryIdKey, categoryId.Value.ToString());
        }

        if (!conditions.Any())
            return new List<ProductCategoryEntity>(0);

        var sqlQueryText = $"SELECT * FROM c WHERE {string.Join(" AND ", conditions)}";

        var affectedProductCategories = await _productCategoryCheckpointRepository.QueryAsync(sqlQueryText, parameters);
        return affectedProductCategories;
    }

    private async Task<List<ProductEntity>> GetAffectedProductsAsync(Guid? categoryId)
    {
        const string categoryIdKey = "@categoryId";

        var conditions = new List<string>();
        var parameters = new Dictionary<string, string>();

        if (categoryId.HasValue)
        {
            conditions.Add($"c.categoryId = {categoryIdKey}");
            parameters.Add(categoryIdKey, categoryId.Value.ToString());
        }

        if (!conditions.Any())
            return new List<ProductEntity>(0);

        var sqlQueryText = $"SELECT * FROM c WHERE {string.Join(" AND ", conditions)}";

        var affectedProducts = await _productCheckpointRepository.QueryAsync(sqlQueryText, parameters);
        return affectedProducts;
    }

    private static string FailedToMessage(UpdateProductWithCategoriesViewCommand command) =>
        $"Failed to update category view '{JsonConvert.SerializeObject(command)}'";
}
