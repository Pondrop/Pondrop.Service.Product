﻿using Pondrop.Service.Models;
using Pondrop.Service.Product.Domain.Models;

namespace Pondrop.Service.ProductCategory.Domain.Models;

public record ProductCategoryRecord(
        Guid Id,
        Guid CategoryId,
        Guid ProductId,
        string PublicationLifecycleId,
        string CreatedBy,
        string UpdatedBy,
        DateTime CreatedUtc,
        DateTime UpdatedUtc,
        DateTime? DeletedUtc)
    : AuditRecord(CreatedBy, UpdatedBy, CreatedUtc, UpdatedUtc, DeletedUtc)
{
    public ProductCategoryRecord() : this(
        Guid.Empty,
        Guid.Empty,
        Guid.Empty,
        string.Empty,
        string.Empty,
        string.Empty,
        DateTime.MinValue,
        DateTime.MinValue,
        null)
    {
    }
}