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

public class UpdateProductWithCategoriesViewCommandHandler : IRequestHandler<UpdateProductWithCategoriesViewCommand, Result<int>>
{
    private readonly ICheckpointRepository<CategoryEntity> _categoryCheckpointRepository;
    private readonly ICheckpointRepository<ProductEntity> _productCheckpointRepository;
    private readonly ICheckpointRepository<ProductCategoryEntity> _productCategoryCheckpointRepository;
    private readonly IContainerRepository<ProductWithCategoryViewRecord> _containerRepository;
    private readonly IMapper _mapper;
    private readonly IUserService _userService;
    private readonly ILogger<UpdateProductWithCategoriesViewCommandHandler> _logger;

    public UpdateProductWithCategoriesViewCommandHandler(
        ICheckpointRepository<CategoryEntity> categoryCheckpointRepository,
        ICheckpointRepository<ProductEntity> productCheckpointRepository,
        ICheckpointRepository<ProductCategoryEntity> productCategoryCheckpointRepository,
        IContainerRepository<ProductWithCategoryViewRecord> containerRepository,
        IMapper mapper,
        IUserService userService,
        ILogger<UpdateProductWithCategoriesViewCommandHandler> logger) : base()
    {
        _categoryCheckpointRepository = categoryCheckpointRepository;
        _productCheckpointRepository = productCheckpointRepository;
        _productCategoryCheckpointRepository = productCategoryCheckpointRepository;
        _containerRepository = containerRepository;
        _mapper = mapper;
        _userService = userService;
        _logger = logger;
    }

    public async Task<Result<int>> Handle(UpdateProductWithCategoriesViewCommand command, CancellationToken cancellationToken)
    {
        if (!command.CategoryId.HasValue && !command.ProductId.HasValue && !command.ProductCategoryId.HasValue)
            return Result<int>.Success(0);

        var result = default(Result<int>);

        Task<ProductEntity?> productTask;
        Task<List<ProductCategoryEntity>> affectedProductCategoriesTask;

        try
        {
            affectedProductCategoriesTask = GetAffectedProductCategoriesAsync(command.CategoryId, command.ProductId, command.ProductCategoryId);

            await Task.WhenAll(affectedProductCategoriesTask);

            if (affectedProductCategoriesTask.Result == null)
                return Result<int>.Success(0);

            Guid productId = Guid.Empty;

            if (command.ProductId.HasValue)
            {
                productId = command.ProductId.Value;
            }
            else
            {
                productId = affectedProductCategoriesTask.Result?.FirstOrDefault()?.ProductId ?? Guid.Empty;
            }

            productTask = _productCheckpointRepository.GetByIdAsync(productId);
            affectedProductCategoriesTask = GetAffectedProductCategoriesAsync(null, productId);

            await Task.WhenAll(productTask, affectedProductCategoriesTask);

            if (productTask.Result == null)
                return Result<int>.Success(0);
            var success = false;

            var productWithCategoryView = _mapper.Map<ProductWithCategoryViewRecord>(productTask.Result);


            foreach (var productCategory in affectedProductCategoriesTask.Result)
            {
                try
                {
                    var affectedCategory = await _categoryCheckpointRepository.GetByIdAsync(productCategory.CategoryId);

                    if (affectedCategory != null)
                        productWithCategoryView.Categories.Add(_mapper.Map<CategoryViewRecord>(affectedCategory));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Failed to update category view for '{productTask.Result.Id}'");
                }
            }

            var upsertResult = await _containerRepository.UpsertAsync(productWithCategoryView);

            result = Result<int>.Success(1);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, FailedToMessage(command));
            result = Result<int>.Error(ex);
        }

        return result;
    }

    private async Task<List<ProductCategoryEntity>> GetAffectedProductCategoriesAsync(Guid? categoryId = null, Guid? productId = null, Guid? productCategoryId = null)
    {
        const string categoryIdKey = "@categoryId";
        const string productIdKey = "@productId";
        const string productCategoryIdKey = "@productCategoryId";

        var conditions = new List<string>();
        var parameters = new Dictionary<string, string>();

        if (categoryId.HasValue)
        {
            conditions.Add($"c.categoryId = {categoryIdKey}");
            parameters.Add(categoryIdKey, categoryId.Value.ToString());
        }

        if (productId.HasValue)
        {
            conditions.Add($"c.productId = {productIdKey}");
            parameters.Add(productIdKey, productId.Value.ToString());
        }

        if (productCategoryId.HasValue)
        {
            conditions.Add($"c.id = {productCategoryIdKey}");
            parameters.Add(productCategoryIdKey, productCategoryId.Value.ToString());
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