
namespace Pondrop.Service.Product.Domain.Models;

public record CategoryViewRecord(
        Guid Id,
        string CategoryName,
        string Description,
        string PublicationLifecycleId,
        string CreatedBy,
        string UpdatedBy,
        DateTime CreatedUtc,
        DateTime UpdatedUtc)
    : CategoryRecord(Id, CategoryName, Description, PublicationLifecycleId, CreatedBy, UpdatedBy, CreatedUtc, UpdatedUtc)
{
    public CategoryViewRecord() : this(
        Guid.Empty,
        string.Empty,
        string.Empty,
        string.Empty,
        string.Empty,
        string.Empty,
        DateTime.MinValue,
        DateTime.MinValue)
    {
    }
}