using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Pondrop.Service.Product.Application.Interfaces;
using Pondrop.Service.Product.Application.Interfaces.Services;
using Pondrop.Service.Product.Application.Models;
using Pondrop.Service.Product.Domain.Events.Category;
using Pondrop.Service.Product.Domain.Models;

namespace Pondrop.Service.Product.Application.Commands;

public class UpdateCategoryViewCommandHandler : IRequestHandler<UpdateCategoryViewCommand, Result<int>>
{
    private readonly ICheckpointRepository<CategoryEntity> _categoryCheckpointRepository;
    private readonly IContainerRepository<CategoryViewRecord> _containerRepository;
    private readonly IMapper _mapper;
    private readonly IUserService _userService;
    private readonly ILogger<UpdateCategoryViewCommandHandler> _logger;

    public UpdateCategoryViewCommandHandler(
        ICheckpointRepository<CategoryEntity> categoryCheckpointRepository,
        IContainerRepository<CategoryViewRecord> containerRepository,
        IMapper mapper,
        IUserService userService,
        ILogger<UpdateCategoryViewCommandHandler> logger) : base()
    {
        _categoryCheckpointRepository = categoryCheckpointRepository;
        _containerRepository = containerRepository;
        _mapper = mapper;
        _userService = userService;
        _logger = logger;
    }

    public async Task<Result<int>> Handle(UpdateCategoryViewCommand command, CancellationToken cancellationToken)
    {
        if (!command.CategoryId.HasValue)
            return Result<int>.Success(0);

        var result = default(Result<int>);

        try
        {
            var affectedStoresTask = GetAffectedCategoriesAsync(command.CategoryId);

            var tasks = affectedStoresTask.Result.Select(async i =>
            {
                var success = false;

                try
                {
                    var categoryView = _mapper.Map<CategoryViewRecord>(i);

                    var result = await _containerRepository.UpsertAsync(categoryView);
                    success = result != null;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Failed to update category view for '{i.Id}'");
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

    private async Task<List<CategoryEntity>> GetAffectedCategoriesAsync(Guid? categoryId)
    {
        const string categoryIdKey = "@categoryId";

        var conditions = new List<string>();
        var parameters = new Dictionary<string, string>();

        if (categoryId.HasValue)
        {
            conditions.Add($"c.id = {categoryIdKey}");
            parameters.Add(categoryIdKey, categoryId.Value.ToString());
        }

        if (!conditions.Any())
            return new List<CategoryEntity>(0);

        var sqlQueryText = $"SELECT * FROM c WHERE {string.Join(" AND ", conditions)}";

        var affectedStores = await _categoryCheckpointRepository.QueryAsync(sqlQueryText, parameters);
        return affectedStores;
    }

    private static string FailedToMessage(UpdateCategoryViewCommand command) =>
        $"Failed to update category view '{JsonConvert.SerializeObject(command)}'";
}