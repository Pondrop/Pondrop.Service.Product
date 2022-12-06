using Pondrop.Service.Models;

namespace Pondrop.Service.Product.Domain.Models;

public record BrandRecord(
        Guid Id,
        string Name,
        Guid CompanyId,
        string WebsiteUrl,
        string Description,
        string PublicationLifecycleId,
        string CreatedBy,
        string UpdatedBy,
        DateTime CreatedUtc,
        DateTime UpdatedUtc,
        DateTime? DeletedUtc)
    : AuditRecord(CreatedBy, UpdatedBy, CreatedUtc, UpdatedUtc, DeletedUtc)
{
    public BrandRecord() : this(
        Guid.Empty,
        string.Empty,
        Guid.Empty,
        string.Empty,
        string.Empty,
        string.Empty,
        string.Empty,
        string.Empty,
        DateTime.MinValue,
        DateTime.MinValue,
        null)
    {
    }
}