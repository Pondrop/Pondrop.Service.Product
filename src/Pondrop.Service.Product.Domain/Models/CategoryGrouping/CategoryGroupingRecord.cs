namespace Pondrop.Service.Product.Domain.Models;

public record CategoryGroupingRecord(
    Guid Id,
    Guid HigherLevelCategoryId,
    Guid LowerLevelCategoryId,
    string Description,
    string PublicationLifecycleId,
    string CreatedBy,
    string UpdatedBy,
    DateTime CreatedUtc,
    DateTime UpdatedUtc)
    : AuditRecord(CreatedBy, UpdatedBy, CreatedUtc, UpdatedUtc)
{
    public CategoryGroupingRecord() : this(
        Guid.Empty,
        Guid.Empty,
        Guid.Empty,
        string.Empty,
        string.Empty,
        string.Empty,
        string.Empty,
        DateTime.MinValue,
        DateTime.MinValue)
    {
    }
}