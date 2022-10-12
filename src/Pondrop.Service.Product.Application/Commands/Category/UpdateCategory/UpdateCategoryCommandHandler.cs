using AutoMapper;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Pondrop.Service.Product.Application.Interfaces;
using Pondrop.Service.Product.Application.Interfaces.Services;
using Pondrop.Service.Product.Application.Models;
using Pondrop.Service.Product.Domain.Events;
using Pondrop.Service.Product.Domain.Events.Category;
using Pondrop.Service.Product.Domain.Models;

namespace Pondrop.Service.Product.Application.Commands;

public class UpdateCategoryCommandHandler : DirtyCommandHandler<CategoryEntity, UpdateCategoryCommand, Result<CategoryRecord>>
{
    private readonly IEventRepository _eventRepository;
    private readonly ICheckpointRepository<CategoryEntity> _categoryCheckpointRepository;
    private readonly IMapper _mapper;
    private readonly IUserService _userService;
    private readonly IValidator<UpdateCategoryCommand> _validator;
    private readonly ILogger<UpdateCategoryCommandHandler> _logger;

    public UpdateCategoryCommandHandler(
        IOptions<CategoryUpdateConfiguration> categoryUpdateConfig,
        IEventRepository eventRepository,
        ICheckpointRepository<CategoryEntity> categoryCheckpointRepository,
        IDaprService daprService,
        IUserService userService,
        IMapper mapper,
        IValidator<UpdateCategoryCommand> validator,
        ILogger<UpdateCategoryCommandHandler> logger) : base(eventRepository, categoryUpdateConfig.Value, daprService, logger)
    {
        _eventRepository = eventRepository;
        _categoryCheckpointRepository = categoryCheckpointRepository;
        _mapper = mapper;
        _userService = userService;
        _validator = validator;
        _logger = logger;
    }

    public override async Task<Result<CategoryRecord>> Handle(UpdateCategoryCommand command, CancellationToken cancellationToken)
    {
        var validation = _validator.Validate(command);

        if (!validation.IsValid)
        {
            var errorMessage = $"Update category failed, errors on validation {validation}";
            _logger.LogError(errorMessage);
            return Result<CategoryRecord>.Error(errorMessage);
        }

        var result = default(Result<CategoryRecord>);

        try
        {
            var categoryEntity = await _categoryCheckpointRepository.GetByIdAsync(command.Id);
            categoryEntity ??= await GetFromStreamAsync(command.Id);


            if (categoryEntity is not null)
            {
                if (!string.IsNullOrEmpty(command.Name))
                {
                    var duplicateMessage = $"Possible category match found";
                    var existingCategory = await GetExistingCategoryByName(command.Id, command.Name);
                    if (existingCategory != null && existingCategory.Count > 0)
                        return Result<CategoryRecord>.Error(duplicateMessage);
                }

                var evtPayload = new UpdateCategory(
                    command.Name,
                    command.Type,
                    command.PublicationLifecycleId);
                var createdBy = _userService.CurrentUserId();

                var success = await UpdateStreamAsync(categoryEntity, evtPayload, createdBy);

                if (!success)
                {
                    await _categoryCheckpointRepository.FastForwardAsync(categoryEntity);
                    success = await UpdateStreamAsync(categoryEntity, evtPayload, createdBy);
                }

                await Task.WhenAll(
                    InvokeDaprMethods(categoryEntity.Id, categoryEntity.GetEvents(categoryEntity.AtSequence)));

                result = success
                    ? Result<CategoryRecord>.Success(_mapper.Map<CategoryRecord>(categoryEntity))
                    : Result<CategoryRecord>.Error(FailedToCreateMessage(command));
            }
            else
            {
                result = Result<CategoryRecord>.Error($"category does not exist '{command.Id}'");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, FailedToCreateMessage(command));
            result = Result<CategoryRecord>.Error(ex);
        }

        return result;
    }
    private async Task<List<CategoryEntity>> GetExistingCategoryByName(Guid id, string categoryName)
    {
        const string categoryNameKey = "@categoryName";
        const string categoryIdKey = "@categoryId";

        var conditions = new List<string>();
        var parameters = new Dictionary<string, string>();

        conditions.Add($"c.name = {categoryNameKey} AND c.id != {categoryIdKey}");
        parameters.Add(categoryNameKey, categoryName);
        parameters.Add(categoryIdKey, id.ToString());

        if (!conditions.Any())
            return new List<CategoryEntity>(0);

        var sqlQueryText = $"SELECT * FROM c WHERE {string.Join(" AND ", conditions)}";

        var affectedProductCategories = await _categoryCheckpointRepository.QueryAsync(sqlQueryText, parameters);
        return affectedProductCategories;
    }
    private static string FailedToCreateMessage(UpdateCategoryCommand command) =>
        $"Failed to update category\nCommand: '{JsonConvert.SerializeObject(command)}'";
}