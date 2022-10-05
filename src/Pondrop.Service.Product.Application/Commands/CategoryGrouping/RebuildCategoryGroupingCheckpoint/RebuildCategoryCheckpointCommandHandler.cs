﻿using Microsoft.Extensions.Logging;
using Pondrop.Service.Product.Application.Commands;
using Pondrop.Service.Product.Application.Interfaces;
using Pondrop.Service.Product.Domain.Models;

namespace Pondrop.Service.Product.Application.Commands;

public class RebuildCategoryGroupingCheckpointCommandHandler : RebuildCheckpointCommandHandler<RebuildCategoryGroupingCheckpointCommand, CategoryGroupingEntity>
{
    public RebuildCategoryGroupingCheckpointCommandHandler(
        ICheckpointRepository<CategoryGroupingEntity> categoryCheckpointRepository,
        ILogger<RebuildCategoryGroupingCheckpointCommandHandler> logger) : base(categoryCheckpointRepository, logger)
    {
    }
}