using Microsoft.Extensions.Logging;
using Pondrop.Service.Interfaces;
using Pondrop.Service.Product.Domain.Models;

namespace Pondrop.Service.Product.Application.Commands;

public class RebuildBrandCheckpointCommandHandler : RebuildCheckpointCommandHandler<RebuildBrandCheckpointCommand, BrandEntity>
{
    public RebuildBrandCheckpointCommandHandler(
        ICheckpointRepository<BrandEntity> brandCheckpointRepository,
        ILogger<RebuildBrandCheckpointCommandHandler> logger) : base(brandCheckpointRepository, logger)
    {
    }
}