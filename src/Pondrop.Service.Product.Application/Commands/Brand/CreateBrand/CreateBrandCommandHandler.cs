using AutoMapper;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Pondrop.Service.Interfaces;
using Pondrop.Service.Interfaces.Services;
using Pondrop.Service.Product.Application.Models;
using Pondrop.Service.Product.Domain.Events.Brand;
using Pondrop.Service.Product.Domain.Models;

namespace Pondrop.Service.Product.Application.Commands;

public class CreateBrandCommandHandler : DirtyCommandHandler<BrandEntity, CreateBrandCommand, Result<BrandRecord>>
{
    private readonly IEventRepository _eventRepository;
    private readonly IMapper _mapper;
    private readonly IUserService _userService;
    private readonly IValidator<CreateBrandCommand> _validator;
    private readonly ILogger<CreateBrandCommandHandler> _logger;

    public CreateBrandCommandHandler(
        IOptions<BrandUpdateConfiguration> brandUpdateConfig,
        IEventRepository eventRepository,
        IDaprService daprService,
        IUserService userService,
        IMapper mapper,
        IValidator<CreateBrandCommand> validator,
        ILogger<CreateBrandCommandHandler> logger) : base(eventRepository, brandUpdateConfig.Value, daprService, logger)
    {
        _eventRepository = eventRepository;
        _mapper = mapper;
        _userService = userService;
        _validator = validator;
        _logger = logger;
    }

    public override async Task<Result<BrandRecord>> Handle(CreateBrandCommand command, CancellationToken cancellationToken)
    {
        var validation = _validator.Validate(command);

        if (!validation.IsValid)
        {
            var errorMessage = $"Create brand failed, errors on validation {validation}";
            _logger.LogError(errorMessage);
            return Result<BrandRecord>.Error(errorMessage);
        }

        var result = default(Result<BrandRecord>);

        try
        {


            var brandEntity = new BrandEntity(
                command.Name,
                command.CompanyId,
                command.WebsiteUrl,
                command.Description,
                command.PublicationLifecycleId,
                _userService.CurrentUserId());
           
            var success = await _eventRepository.AppendEventsAsync(brandEntity.StreamId, 0, brandEntity.GetEvents());

            await Task.WhenAll(
                InvokeDaprMethods(brandEntity.Id, brandEntity.GetEvents()));

            result = success
                ? Result<BrandRecord>.Success(_mapper.Map<BrandRecord>(brandEntity))
                : Result<BrandRecord>.Error(FailedToCreateMessage(command));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, FailedToCreateMessage(command));
            result = Result<BrandRecord>.Error(ex);
        }

        return result;
    }

    private static string FailedToCreateMessage(CreateBrandCommand command) =>
        $"Failed to create brand\nCommand: '{JsonConvert.SerializeObject(command)}'";
}