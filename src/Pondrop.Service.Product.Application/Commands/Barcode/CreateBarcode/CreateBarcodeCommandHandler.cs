using AutoMapper;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Pondrop.Service.Product.Application.Interfaces;
using Pondrop.Service.Product.Application.Interfaces.Services;
using Pondrop.Service.Product.Application.Models;
using Pondrop.Service.Product.Domain.Events.Barcode;
using Pondrop.Service.Product.Domain.Models;

namespace Pondrop.Service.Product.Application.Commands;

public class CreateBarcodeCommandHandler : DirtyCommandHandler<BarcodeEntity, CreateBarcodeCommand, Result<BarcodeRecord>>
{
    private readonly IEventRepository _eventRepository;
    private readonly IMapper _mapper;
    private readonly IUserService _userService;
    private readonly IValidator<CreateBarcodeCommand> _validator;
    private readonly ILogger<CreateBarcodeCommandHandler> _logger;

    public CreateBarcodeCommandHandler(
        IOptions<BarcodeUpdateConfiguration> barcodeUpdateConfig,
        IEventRepository eventRepository,
        IDaprService daprService,
        IUserService userService,
        IMapper mapper,
        IValidator<CreateBarcodeCommand> validator,
        ILogger<CreateBarcodeCommandHandler> logger) : base(eventRepository, barcodeUpdateConfig.Value, daprService, logger)
    {
        _eventRepository = eventRepository;
        _mapper = mapper;
        _userService = userService;
        _validator = validator;
        _logger = logger;
    }

    public override async Task<Result<BarcodeRecord>> Handle(CreateBarcodeCommand command, CancellationToken cancellationToken)
    {
        var validation = _validator.Validate(command);

        if (!validation.IsValid)
        {
            var errorMessage = $"Create barcode failed, errors on validation {validation}";
            _logger.LogError(errorMessage);
            return Result<BarcodeRecord>.Error(errorMessage);
        }

        var result = default(Result<BarcodeRecord>);

        try
        {


            var barcodeEntity = new BarcodeEntity(
                command.BarcodeNumber,
                command.BarcodeText,
                command.BarcodeType,
                command.ProductId,
                command.RetailerId,
                command.CompanyId,
                command.PublicationLifecycleId,
                _userService.CurrentUserId());
           
            var success = await _eventRepository.AppendEventsAsync(barcodeEntity.StreamId, 0, barcodeEntity.GetEvents());

            await Task.WhenAll(
                InvokeDaprMethods(barcodeEntity.Id, barcodeEntity.GetEvents()));

            result = success
                ? Result<BarcodeRecord>.Success(_mapper.Map<BarcodeRecord>(barcodeEntity))
                : Result<BarcodeRecord>.Error(FailedToCreateMessage(command));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, FailedToCreateMessage(command));
            result = Result<BarcodeRecord>.Error(ex);
        }

        return result;
    }

    private static string FailedToCreateMessage(CreateBarcodeCommand command) =>
        $"Failed to create barcode\nCommand: '{JsonConvert.SerializeObject(command)}'";
}