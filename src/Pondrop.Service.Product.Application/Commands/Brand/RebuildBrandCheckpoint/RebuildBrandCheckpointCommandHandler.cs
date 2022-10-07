using Microsoft.Extensions.Logging;
using Pondrop.Service.Product.Application.Commands;
using Pondrop.Service.Product.Application.Interfaces;
using Pondrop.Service.Product.Domain.Models;

namespace Pondrop.Service.Product.Application.Commands;

public class RebuildBrandCheckpointCommandHandler : RebuildCheckpointCommandHandler<RebuildBrandCheckpointCommand, BrandEntity>
{
    public RebuildBrandCheckpointCommandHandler(
        ICheckpointRepository<BrandEntity> categoryCheckpointRepository,
        ILogger<RebuildBrandCheckpointCommandHandler> logger) : base(categoryCheckpointRepository, logger)
    {
    }
}