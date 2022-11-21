using AutoMapper;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Pondrop.Service.Interfaces;
using Pondrop.Service.Interfaces.Services;
using Pondrop.Service.Product.Application.Models;
using Pondrop.Service.Product.Domain.Events.Barcode;
using Pondrop.Service.Product.Domain.Models;

namespace Pondrop.Service.Product.Application.Commands;

public class UpdateBarcodeCommandHandler : DirtyCommandHandler<BarcodeEntity, UpdateBarcodeCommand, Result<BarcodeRecord>>
{
    private readonly IEventRepository _eventRepository;
    private readonly ICheckpointRepository<BarcodeEntity> _barcodeCheckpointRepository;
    private readonly IMapper _mapper;
    private readonly IUserService _userService;
    private readonly IValidator<UpdateBarcodeCommand> _validator;
    private readonly ILogger<UpdateBarcodeCommandHandler> _logger;

    public UpdateBarcodeCommandHandler(
        IOptions<BarcodeUpdateConfiguration> barcodeUpdateConfig,
        IEventRepository eventRepository,
        ICheckpointRepository<BarcodeEntity> barcodeCheckpointRepository,
        IDaprService daprService,
        IUserService userService,
        IMapper mapper,
        IValidator<UpdateBarcodeCommand> validator,
        ILogger<UpdateBarcodeCommandHandler> logger) : base(eventRepository, barcodeUpdateConfig.Value, daprService, logger)
    {
        _eventRepository = eventRepository;
        _barcodeCheckpointRepository = barcodeCheckpointRepository;
        _mapper = mapper;
        _userService = userService;
        _validator = validator;
        _logger = logger;
    }

    public override async Task<Result<BarcodeRecord>> Handle(UpdateBarcodeCommand command, CancellationToken cancellationToken)
    {
        var validation = _validator.Validate(command);

        if (!validation.IsValid)
        {
            var errorMessage = $"Update barcode failed, errors on validation {validation}";
            _logger.LogError(errorMessage);
            return Result<BarcodeRecord>.Error(errorMessage);
        }

        var result = default(Result<BarcodeRecord>);

        try
        {
            var barcodeEntity = await _barcodeCheckpointRepository.GetByIdAsync(command.Id);
            barcodeEntity ??= await GetFromStreamAsync(command.Id);

            if (barcodeEntity is not null)
            {
                var evtPayload = new UpdateBarcode(
                    command.BarcodeNumber,
                    command.BarcodeText,
                    command.BarcodeType,
                    command.ProductId,
                    command.RetailerId,
                    command.CompanyId,
                    command.PublicationLifecycleId);
                var createdBy = _userService.CurrentUserId();

                var success = await UpdateStreamAsync(barcodeEntity, evtPayload, createdBy);

                if (!success)
                {
                    await _barcodeCheckpointRepository.FastForwardAsync(barcodeEntity);
                    success = await UpdateStreamAsync(barcodeEntity, evtPayload, createdBy);
                }

                await Task.WhenAll(
                    InvokeDaprMethods(barcodeEntity.Id, barcodeEntity.GetEvents(barcodeEntity.AtSequence)));

                result = success
                    ? Result<BarcodeRecord>.Success(_mapper.Map<BarcodeRecord>(barcodeEntity))
                    : Result<BarcodeRecord>.Error(FailedToCreateMessage(command));
            }
            else
            {
                result = Result<BarcodeRecord>.Error($"barcode does not exist '{command.Id}'");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, FailedToCreateMessage(command));
            result = Result<BarcodeRecord>.Error(ex);
        }

        return result;
    }

    private static string FailedToCreateMessage(UpdateBarcodeCommand command) =>
        $"Failed to update barcode\nCommand: '{JsonConvert.SerializeObject(command)}'";
}