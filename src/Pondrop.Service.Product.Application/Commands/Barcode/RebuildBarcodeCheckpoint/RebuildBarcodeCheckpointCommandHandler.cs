using Microsoft.Extensions.Logging;
using Pondrop.Service.Product.Application.Commands;
using Pondrop.Service.Product.Application.Interfaces;
using Pondrop.Service.Product.Domain.Models;

namespace Pondrop.Service.Product.Application.Commands;

public class RebuildBarcodeCheckpointCommandHandler : RebuildCheckpointCommandHandler<RebuildBarcodeCheckpointCommand, BarcodeEntity>
{
    public RebuildBarcodeCheckpointCommandHandler(
        ICheckpointRepository<BarcodeEntity> barcodeCheckpointRepository,
        ILogger<RebuildBarcodeCheckpointCommandHandler> logger) : base(barcodeCheckpointRepository, logger)
    {
    }
}