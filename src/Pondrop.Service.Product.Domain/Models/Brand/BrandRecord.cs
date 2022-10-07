namespace Pondrop.Service.Product.Domain.Models;

public record BrandRecord(
        Guid Id,
        string Name,
        Guid CompanyId,
        string PublicationLifecycleId,
        string CreatedBy,
        string UpdatedBy,
        DateTime CreatedUtc,
        DateTime UpdatedUtc)
    : AuditRecord(CreatedBy, UpdatedBy, CreatedUtc, UpdatedUtc)
{
    public BrandRecord() : this(
        Guid.Empty,
        string.Empty,
        Guid.Empty,
        string.Empty,
        string.Empty,
        string.Empty,
        DateTime.MinValue,
        DateTime.MinValue)
    {
    }
}