using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Pondrop.Service.Interfaces;
using Pondrop.Service.Interfaces.Services;
using Pondrop.Service.Product.Application.Models;
using Pondrop.Service.Product.Domain.Models;

namespace Pondrop.Service.Product.Application.Commands;

public class UpdateCategoryGroupingViewCommandHandler : IRequestHandler<UpdateCategoryGroupingViewCommand, Result<int>>
{
    private readonly ICheckpointRepository<CategoryGroupingEntity> _categoryGroupingCheckpointRepository;
    private readonly ICheckpointRepository<CategoryEntity> _categoryCheckpointRepository;
    private readonly IContainerRepository<CategoryGroupingViewRecord> _containerRepository;
    private readonly IMapper _mapper;
    private readonly IUserService _userService;
    private readonly ILogger<UpdateCategoryGroupingViewCommandHandler> _logger;

    public UpdateCategoryGroupingViewCommandHandler(
        ICheckpointRepository<CategoryGroupingEntity> categoryGroupingCheckpointRepository,
        ICheckpointRepository<CategoryEntity> categoryCheckpointRepository,
        IContainerRepository<CategoryGroupingViewRecord> containerRepository,
        IMapper mapper,
        IUserService userService,
        ILogger<UpdateCategoryGroupingViewCommandHandler> logger) : base()
    {
        _categoryGroupingCheckpointRepository = categoryGroupingCheckpointRepository;
        _categoryCheckpointRepository = categoryCheckpointRepository;
        _containerRepository = containerRepository;
        _mapper = mapper;
        _userService = userService;
        _logger = logger;
    }

    public async Task<Result<int>> Handle(UpdateCategoryGroupingViewCommand command, CancellationToken cancellationToken)
    {
        if (!command.CategoryId.HasValue && !command.CategoryGroupingId.HasValue)
            return Result<int>.Success(0);

        var result = default(Result<int>);

        try
        {
            var affectedCategoryGroupingTask = GetAffectedCategoryGroupingAsync(command.CategoryGroupingId, command.CategoryId);
            var categoriesTask = _categoryCheckpointRepository.GetAllAsync();

            await Task.WhenAll(affectedCategoryGroupingTask, categoriesTask);

            var categories = categoriesTask.Result;

            var tasks = affectedCategoryGroupingTask.Result.Select(async i =>
            {
                var success = false;

                try
                {
                    var parentCategory = categories?.FirstOrDefault(c => c.Id == i.HigherLevelCategoryId);
                    var childCategory = categories?.FirstOrDefault(c => c.Id == i.LowerLevelCategoryId);

                    var categoryGrouping = new CategoryGroupingViewRecord(i.Id, i.HigherLevelCategoryId, parentCategory.Name, i.LowerLevelCategoryId, childCategory.Name);

                    var result = await _containerRepository.UpsertAsync(categoryGrouping);
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

    private static string FailedToMessage(UpdateCategoryGroupingViewCommand command) =>
        $"Failed to update category view '{JsonConvert.SerializeObject(command)}'";
}