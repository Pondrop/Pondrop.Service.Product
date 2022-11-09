using Microsoft.Extensions.Logging;
using Pondrop.Service.Interfaces;
using Pondrop.Service.Product.Domain.Models.Product;

namespace Pondrop.Service.Product.Application.Commands;

public class RebuildProductCheckpointCommandHandler : RebuildCheckpointCommandHandler<RebuildProductCheckpointCommand, ProductEntity>
{
    public RebuildProductCheckpointCommandHandler(
        ICheckpointRepository<ProductEntity> ProductCheckpointRepository,
        ILogger<RebuildProductCheckpointCommandHandler> logger) : base(ProductCheckpointRepository, logger)
    {
    }
}