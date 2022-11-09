using AutoMapper;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Pondrop.Service.Interfaces;
using Pondrop.Service.Product.Domain.Models;

namespace Pondrop.Service.Product.Application.Commands;

public class UpdateBarcodeCheckpointByIdCommandHandler : UpdateCheckpointByIdCommandHandler<UpdateBarcodeCheckpointByIdCommand, BarcodeEntity, BarcodeRecord>
{
    public UpdateBarcodeCheckpointByIdCommandHandler(
        IEventRepository eventRepository,
        ICheckpointRepository<BarcodeEntity> barcodeCheckpointRepository,
        IMapper mapper,
        IValidator<UpdateCheckpointByIdCommand> validator,
        ILogger<UpdateBarcodeCheckpointByIdCommandHandler> logger) : base(eventRepository, barcodeCheckpointRepository, mapper, validator, logger)
    {
    }
}