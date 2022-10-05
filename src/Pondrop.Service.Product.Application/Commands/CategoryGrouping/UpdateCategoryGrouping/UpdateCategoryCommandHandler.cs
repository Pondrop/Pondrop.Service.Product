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
using Pondrop.Service.Product.Domain.Events.CategoryGrouping;
using Pondrop.Service.Product.Domain.Models;

namespace Pondrop.Service.Product.Application.Commands;

public class UpdateCategoryGroupingCommandHandler : DirtyCommandHandler<CategoryGroupingEntity, UpdateCategoryGroupingCommand, Result<CategoryGroupingRecord>>
{
    private readonly IEventRepository _eventRepository;
    private readonly ICheckpointRepository<CategoryGroupingEntity> _categoryGroupingCheckpointRepository;
    private readonly IMapper _mapper;
    private readonly IUserService _userService;
    private readonly IValidator<UpdateCategoryGroupingCommand> _validator;
    private readonly ILogger<UpdateCategoryGroupingCommandHandler> _logger;

    public UpdateCategoryGroupingCommandHandler(
        IOptions<CategoryUpdateConfiguration> categoryUpdateConfig,
        IEventRepository eventRepository,
        ICheckpointRepository<CategoryGroupingEntity> categoryGroupingCheckpointRepository,
        IDaprService daprService,
        IUserService userService,
        IMapper mapper,
        IValidator<UpdateCategoryGroupingCommand> validator,
        ILogger<UpdateCategoryGroupingCommandHandler> logger) : base(eventRepository, categoryUpdateConfig.Value, daprService, logger)
    {
        _eventRepository = eventRepository;
        _categoryGroupingCheckpointRepository = categoryGroupingCheckpointRepository;
        _mapper = mapper;
        _userService = userService;
        _validator = validator;
        _logger = logger;
    }

    public override async Task<Result<CategoryGroupingRecord>> Handle(UpdateCategoryGroupingCommand command, CancellationToken cancellationToken)
    {
        var validation = _validator.Validate(command);

        if (!validation.IsValid)
        {
            var errorMessage = $"Update category grouping failed, errors on validation {validation}";
            _logger.LogError(errorMessage);
            return Result<CategoryGroupingRecord>.Error(errorMessage);
        }

        var result = default(Result<CategoryGroupingRecord>);

        try
        {
            var categoryEntity = await _categoryGroupingCheckpointRepository.GetByIdAsync(command.Id);
            categoryEntity ??= await GetFromStreamAsync(command.Id);

            if (categoryEntity is not null)
            {
                var evtPayload = new UpdateCategoryGrouping(
                    command.HigherLevelCategoryId,
                    command.LowerLevelCategoryId,
                    command.Description,
                    command.PublicationLifecycleId);
                var createdBy = _userService.CurrentUserId();

                var success = await UpdateStreamAsync(categoryEntity, evtPayload, createdBy);

                if (!success)
                {
                    await _categoryGroupingCheckpointRepository.FastForwardAsync(categoryEntity);
                    success = await UpdateStreamAsync(categoryEntity, evtPayload, createdBy);
                }

                await Task.WhenAll(
                    InvokeDaprMethods(categoryEntity.Id, categoryEntity.GetEvents(categoryEntity.AtSequence)));

                result = success
                    ? Result<CategoryGroupingRecord>.Success(_mapper.Map<CategoryGroupingRecord>(categoryEntity))
                    : Result<CategoryGroupingRecord>.Error(FailedToCreateMessage(command));
            }
            else
            {
                result = Result<CategoryGroupingRecord>.Error($"category grouping does not exist '{command.Id}'");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, FailedToCreateMessage(command));
            result = Result<CategoryGroupingRecord>.Error(ex);
        }

        return result;
    }

    private static string FailedToCreateMessage(UpdateCategoryGroupingCommand command) =>
        $"Failed to update category grouping\nCommand: '{JsonConvert.SerializeObject(command)}'";
}