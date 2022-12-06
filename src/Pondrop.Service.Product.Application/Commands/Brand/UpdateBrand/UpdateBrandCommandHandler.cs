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

public class UpdateBrandCommandHandler : DirtyCommandHandler<BrandEntity, UpdateBrandCommand, Result<BrandRecord>>
{
    private readonly IEventRepository _eventRepository;
    private readonly ICheckpointRepository<BrandEntity> _brandCheckpointRepository;
    private readonly IMapper _mapper;
    private readonly IUserService _userService;
    private readonly IValidator<UpdateBrandCommand> _validator;
    private readonly ILogger<UpdateBrandCommandHandler> _logger;

    public UpdateBrandCommandHandler(
        IOptions<BrandUpdateConfiguration> brandUpdateConfig,
        IEventRepository eventRepository,
        ICheckpointRepository<BrandEntity> brandCheckpointRepository,
        IDaprService daprService,
        IUserService userService,
        IMapper mapper,
        IValidator<UpdateBrandCommand> validator,
        ILogger<UpdateBrandCommandHandler> logger) : base(eventRepository, brandUpdateConfig.Value, daprService, logger)
    {
        _eventRepository = eventRepository;
        _brandCheckpointRepository = brandCheckpointRepository;
        _mapper = mapper;
        _userService = userService;
        _validator = validator;
        _logger = logger;
    }

    public override async Task<Result<BrandRecord>> Handle(UpdateBrandCommand command, CancellationToken cancellationToken)
    {
        var validation = _validator.Validate(command);

        if (!validation.IsValid)
        {
            var errorMessage = $"Update brand failed, errors on validation {validation}";
            _logger.LogError(errorMessage);
            return Result<BrandRecord>.Error(errorMessage);
        }

        var result = default(Result<BrandRecord>);

        try
        {
            var brandEntity = await _brandCheckpointRepository.GetByIdAsync(command.Id);
            brandEntity ??= await GetFromStreamAsync(command.Id);

            if (brandEntity is not null)
            {
                var evtPayload = new UpdateBrand(
                command.Name,
                command.CompanyId,
                command.WebsiteUrl,
                command.Description,
                    command.PublicationLifecycleId);
                var createdBy = _userService.CurrentUserId();

                var success = await UpdateStreamAsync(brandEntity, evtPayload, createdBy);

                if (!success)
                {
                    await _brandCheckpointRepository.FastForwardAsync(brandEntity);
                    success = await UpdateStreamAsync(brandEntity, evtPayload, createdBy);
                }

                await Task.WhenAll(
                    InvokeDaprMethods(brandEntity.Id, brandEntity.GetEvents(brandEntity.AtSequence)));

                result = success
                    ? Result<BrandRecord>.Success(_mapper.Map<BrandRecord>(brandEntity))
                    : Result<BrandRecord>.Error(FailedToCreateMessage(command));
            }
            else
            {
                result = Result<BrandRecord>.Error($"brand does not exist '{command.Id}'");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, FailedToCreateMessage(command));
            result = Result<BrandRecord>.Error(ex);
        }

        return result;
    }

    private static string FailedToCreateMessage(UpdateBrandCommand command) =>
        $"Failed to update brand\nCommand: '{JsonConvert.SerializeObject(command)}'";
}