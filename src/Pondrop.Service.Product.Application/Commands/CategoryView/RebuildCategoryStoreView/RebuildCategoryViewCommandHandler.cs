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

public class RebuildCategoryViewCommandHandler : IRequestHandler<RebuildCategoryViewCommand, Result<int>>
{
    private readonly ICheckpointRepository<CategoryEntity> _categoryCheckpointRepository;
    private readonly IContainerRepository<CategoryViewRecord> _containerRepository;
    private readonly IMapper _mapper;
    private readonly IUserService _userService;
    private readonly ILogger<RebuildCategoryViewCommandHandler> _logger;

    public RebuildCategoryViewCommandHandler(
        ICheckpointRepository<CategoryEntity> categoryCheckpointRepository,
        IContainerRepository<CategoryViewRecord> containerRepository,
        IMapper mapper,
        IUserService userService,
        ILogger<RebuildCategoryViewCommandHandler> logger) : base()
    {
        _categoryCheckpointRepository = categoryCheckpointRepository;
        _containerRepository = containerRepository;
        _mapper = mapper;
        _userService = userService;
        _logger = logger;
    }

    public async Task<Result<int>> Handle(RebuildCategoryViewCommand command, CancellationToken cancellationToken)
    {
        var result = default(Result<int>);

        try
        {
            var categorysTask = _categoryCheckpointRepository.GetAllAsync();

            await Task.WhenAll(categorysTask);

            var tasks = categorysTask.Result.Select(async i =>
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
            _logger.LogError(ex, $"Failed to rebuild category view");
            result = Result<int>.Error(ex);
        }

        return result;
    }
}