
namespace Pondrop.Service.Product.Domain.Models;

public record CategoryViewRecord(
        Guid Id,
        string Name,
        string Type,
        string PublicationLifecycleId,
        string CreatedBy,
        string UpdatedBy,
        DateTime CreatedUtc,
        DateTime UpdatedUtc)
    : CategoryRecord(Id, Name, Type, PublicationLifecycleId, CreatedBy, UpdatedBy, CreatedUtc, UpdatedUtc)
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