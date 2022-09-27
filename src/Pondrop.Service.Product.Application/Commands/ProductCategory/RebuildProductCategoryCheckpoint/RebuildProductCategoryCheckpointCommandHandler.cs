using Microsoft.Extensions.Logging;
using Pondrop.Service.Product.Application.Commands;
using Pondrop.Service.Product.Application.Interfaces;
using Pondrop.Service.ProductCategory.Domain.Models;

namespace Pondrop.Service.Product.Application.Commands;

public class RebuildProductCategoryCheckpointCommandHandler : RebuildCheckpointCommandHandler<RebuildProductCategoryCheckpointCommand, ProductCategoryEntity>
{
    public RebuildProductCategoryCheckpointCommandHandler(
        ICheckpointRepository<ProductCategoryEntity> ProductCategoryCheckpointRepository,
        ILogger<RebuildProductCategoryCheckpointCommandHandler> logger) : base(ProductCategoryCheckpointRepository, logger)
    {
    }
}