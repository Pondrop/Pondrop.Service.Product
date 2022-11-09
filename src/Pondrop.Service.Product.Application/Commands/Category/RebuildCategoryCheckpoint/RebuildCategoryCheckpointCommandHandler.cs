using Microsoft.Extensions.Logging;
using Pondrop.Service.Interfaces;
using Pondrop.Service.Product.Domain.Models;

namespace Pondrop.Service.Product.Application.Commands;

public class RebuildCategoryCheckpointCommandHandler : RebuildCheckpointCommandHandler<RebuildCategoryCheckpointCommand, CategoryEntity>
{
    public RebuildCategoryCheckpointCommandHandler(
        ICheckpointRepository<CategoryEntity> categoryCheckpointRepository,
        ILogger<RebuildCategoryCheckpointCommandHandler> logger) : base(categoryCheckpointRepository, logger)
    {
    }
}