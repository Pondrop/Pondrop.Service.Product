using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Pondrop.Service.Interfaces;
using Pondrop.Service.Interfaces.Services;
using Pondrop.Service.Product.Application.Models;
using Pondrop.Service.Product.Domain.Events.Category;
using Pondrop.Service.Product.Domain.Models;

namespace Pondrop.Service.Product.Application.Commands;

public class RebuildCategoryGroupingViewCommandHandler : IRequestHandler<RebuildCategoryGroupingViewCommand, Result<int>>
{
    private readonly ICheckpointRepository<CategoryGroupingEntity> _categoryGroupingCheckpointRepository;
    private readonly ICheckpointRepository<CategoryEntity> _categoryCheckpointRepository;
    private readonly IContainerRepository<CategoryGroupingViewRecord> _containerRepository;
    private readonly IMapper _mapper;
    private readonly IUserService _userService;
    private readonly ILogger<RebuildCategoryGroupingViewCommandHandler> _logger;

    public RebuildCategoryGroupingViewCommandHandler(
        ICheckpointRepository<CategoryGroupingEntity> categoryGroupingCheckpointRepository,
        ICheckpointRepository<CategoryEntity> categoryCheckpointRepository,
        IContainerRepository<CategoryGroupingViewRecord> containerRepository,
        IMapper mapper,
        IUserService userService,
        ILogger<RebuildCategoryGroupingViewCommandHandler> logger) : base()
    {
        _categoryGroupingCheckpointRepository = categoryGroupingCheckpointRepository;
        _categoryCheckpointRepository = categoryCheckpointRepository;
        _containerRepository = containerRepository;
        _mapper = mapper;
        _userService = userService;
        _logger = logger;
    }

    public async Task<Result<int>> Handle(RebuildCategoryGroupingViewCommand command, CancellationToken cancellationToken)
    {
        var result = default(Result<int>);

        try
        {
            var categoryGroupingsTask = _categoryGroupingCheckpointRepository.GetAllAsync();
            var categoriesTask = _categoryCheckpointRepository.GetAllAsync();

            await Task.WhenAll(categoryGroupingsTask, categoriesTask);

            var categories = categoriesTask.Result;

            var tasks = categoryGroupingsTask.Result.Select(async i =>
            {
                var success = false;

                try
                {
                    var parentCategory = categories?.FirstOrDefault(c => c.Id == i.HigherLevelCategoryId);
                    var childCategory = categories?.FirstOrDefault(c => c.Id == i.LowerLevelCategoryId);

                    var categoryGroupingView = new CategoryGroupingViewRecord(i.Id, i.HigherLevelCategoryId, parentCategory?.Name ?? String.Empty, i.LowerLevelCategoryId, childCategory?.Name ?? String.Empty);

                    var result = await _containerRepository.UpsertAsync(categoryGroupingView);
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
            _logger.LogError(ex, $"Failed to rebuild category view");
            result = Result<int>.Error(ex);
        }

        return result;
    }
}