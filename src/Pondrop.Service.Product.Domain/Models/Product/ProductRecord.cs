using Pondrop.Service.Models;

namespace Pondrop.Service.Product.Domain.Models.Product;

public record ProductRecord(
    Guid Id,
    string Name,
    Guid BrandId,
    string ExternalReferenceId,
    string Variant,
    string AltName,
    string ShortDescription,
    double NetContent,
    string NetContentUom,
    string PossibleCategories,
    string PublicationLifecycleId,
    List<Guid> ChildProductId,
        string CreatedBy,
        string UpdatedBy,
        DateTime CreatedUtc,
        DateTime UpdatedUtc)
    : AuditRecord(CreatedBy, UpdatedBy, CreatedUtc, UpdatedUtc)
{
    public ProductRecord() : this(
        Guid.Empty,
        string.Empty,
        Guid.Empty,
        string.Empty,
        string.Empty,
        string.Empty,
        string.Empty,
        0,
        string.Empty,
        string.Empty,
        string.Empty,
        new List<Guid>(),
        string.Empty,
        string.Empty,
        DateTime.MinValue,
        DateTime.MinValue)
    {
    }
}