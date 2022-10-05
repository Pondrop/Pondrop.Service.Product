using AutoMapper;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Pondrop.Service.Product.Application.Interfaces;
using Pondrop.Service.Product.Application.Interfaces.Services;
using Pondrop.Service.Product.Application.Models;
using Pondrop.Service.Product.Domain.Events.Category;
using Pondrop.Service.Product.Domain.Models;

namespace Pondrop.Service.Product.Application.Commands;

public class CreateCategoryGroupingCommandHandler : DirtyCommandHandler<CategoryEntity, CreateCategoryGroupingCommand, Result<CategoryGroupingRecord>>
{
    private readonly IEventRepository _eventRepository;
    private readonly IMapper _mapper;
    private readonly IUserService _userService;
    private readonly IValidator<CreateCategoryGroupingCommand> _validator;
    private readonly ILogger<CreateCategoryGroupingCommandHandler> _logger;

    public CreateCategoryGroupingCommandHandler(
        IOptions<CategoryUpdateConfiguration> categoryUpdateConfig,
        IEventRepository eventRepository,
        IDaprService daprService,
        IUserService userService,
        IMapper mapper,
        IValidator<CreateCategoryGroupingCommand> validator,
        ILogger<CreateCategoryGroupingCommandHandler> logger) : base(eventRepository, categoryUpdateConfig.Value, daprService, logger)
    {
        _eventRepository = eventRepository;
        _mapper = mapper;
        _userService = userService;
        _validator = validator;
        _logger = logger;
    }

    public override async Task<Result<CategoryGroupingRecord>> Handle(CreateCategoryGroupingCommand command, CancellationToken cancellationToken)
    {
        var validation = _validator.Validate(command);

        if (!validation.IsValid)
        {
            var errorMessage = $"Create category failed, errors on validation {validation}";
            _logger.LogError(errorMessage);
            return Result<CategoryGroupingRecord>.Error(errorMessage);
        }

        var result = default(Result<CategoryGroupingRecord>);

        try
        {
            var categoryEntity = new CategoryGroupingEntity(
                command.HigherLevelCategoryId ?? Guid.Empty,
                command.LowerLevelCategoryId ?? Guid.Empty,
                command.Description,
                command.PublicationLifecycleId,
                _userService.CurrentUserId());

            var success = await _eventRepository.AppendEventsAsync(categoryEntity.StreamId, 0, categoryEntity.GetEvents());

            await Task.WhenAll(
                InvokeDaprMethods(categoryEntity.Id, categoryEntity.GetEvents()));

            result = success
                ? Result<CategoryGroupingRecord>.Success(_mapper.Map<CategoryGroupingRecord>(categoryEntity))
                : Result<CategoryGroupingRecord>.Error(FailedToCreateMessage(command));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, FailedToCreateMessage(command));
            result = Result<CategoryGroupingRecord>.Error(ex);
        }

        return result;
    }

    private static string FailedToCreateMessage(CreateCategoryGroupingCommand command) =>
        $"Failed to create category grouping\nCommand: '{JsonConvert.SerializeObject(command)}'";
}