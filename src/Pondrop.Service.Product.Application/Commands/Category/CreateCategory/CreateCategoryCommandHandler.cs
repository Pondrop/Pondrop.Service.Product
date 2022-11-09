using AutoMapper;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Pondrop.Service.Interfaces;
using Pondrop.Service.Interfaces.Services;
using Pondrop.Service.Product.Application.Models;
using Pondrop.Service.Product.Domain.Models;

namespace Pondrop.Service.Product.Application.Commands;

public class CreateCategoryCommandHandler : DirtyCommandHandler<CategoryEntity, CreateCategoryCommand, Result<CategoryRecord>>
{
    private readonly IEventRepository _eventRepository;
    private readonly IMapper _mapper;
    private readonly ICheckpointRepository<CategoryEntity> _checkpointRepository;
    private readonly IUserService _userService;
    private readonly IValidator<CreateCategoryCommand> _validator;
    private readonly ILogger<CreateCategoryCommandHandler> _logger;

    public CreateCategoryCommandHandler(
        IOptions<CategoryUpdateConfiguration> categoryUpdateConfig,
        ICheckpointRepository<CategoryEntity> checkpointRepository,
        IEventRepository eventRepository,
        IDaprService daprService,
        IUserService userService,
        IMapper mapper,
        IValidator<CreateCategoryCommand> validator,
        ILogger<CreateCategoryCommandHandler> logger) : base(eventRepository, categoryUpdateConfig.Value, daprService, logger)
    {
        _eventRepository = eventRepository;
        _mapper = mapper;
        _checkpointRepository = checkpointRepository;
        _userService = userService;
        _validator = validator;
        _logger = logger;
    }

    public override async Task<Result<CategoryRecord>> Handle(CreateCategoryCommand command, CancellationToken cancellationToken)
    {
        var validation = _validator.Validate(command);

        if (!validation.IsValid)
        {
            var errorMessage = $"Create category failed, errors on validation {validation}";
            _logger.LogError(errorMessage);
            return Result<CategoryRecord>.Error(errorMessage);
        }

        var result = default(Result<CategoryRecord>);

        try
        {
            var duplicateMessage = $"Possible category match found";
            var existingCategory = await GetExistingCategoryByName(command.Name);
            if (existingCategory != null && existingCategory.Count > 0)
                return Result<CategoryRecord>.Error(duplicateMessage);


            var categoryEntity = new CategoryEntity(
                    command.Name,
                    command.Type,
                    command.PublicationLifecycleId,
                    _userService.CurrentUserId());

            var success = await _eventRepository.AppendEventsAsync(categoryEntity.StreamId, 0, categoryEntity.GetEvents());

            await Task.WhenAll(
                InvokeDaprMethods(categoryEntity.Id, categoryEntity.GetEvents()));

            result = success
                ? Result<CategoryRecord>.Success(_mapper.Map<CategoryRecord>(categoryEntity))
                : Result<CategoryRecord>.Error(FailedToCreateMessage(command));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, FailedToCreateMessage(command));
            result = Result<CategoryRecord>.Error(ex);
        }

        return result;
    }

    private async Task<List<CategoryEntity>> GetExistingCategoryByName(string categoryName)
    {
        const string categoryNameKey = "@categoryName";

        var conditions = new List<string>();
        var parameters = new Dictionary<string, string>();

        conditions.Add($"LOWER(c.name) = {categoryNameKey}");
        parameters.Add(categoryNameKey, categoryName.ToLower());

        if (!conditions.Any())
            return new List<CategoryEntity>(0);

        var sqlQueryText = $"SELECT * FROM c WHERE {string.Join(" AND ", conditions)}";

        var affectedProductCategories = await _checkpointRepository.QueryAsync(sqlQueryText, parameters);
        return affectedProductCategories;
    }


    private static string FailedToCreateMessage(CreateCategoryCommand command) =>
        $"Failed to create category\nCommand: '{JsonConvert.SerializeObject(command)}'";
}