using Microsoft.Extensions.Logging;
using Pondrop.Service.Product.Application.Commands;
using Pondrop.Service.Product.Application.Interfaces;
using Pondrop.Service.Product.Domain.Models;
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