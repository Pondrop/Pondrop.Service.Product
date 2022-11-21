using Pondrop.Service.Models;

namespace Pondrop.Service.Product.Domain.Models;

public record BarcodeRecord(
    Guid Id,
    string BarcodeNumber,
    string BarcodeText,
    string BarcodeType,
    Guid ProductID,
    Guid RetailerID,
    Guid CompanyID,
    string CreatedBy,
    string UpdatedBy,
    DateTime CreatedUtc,
    DateTime UpdatedUtc,
        DateTime? DeletedUtc)
    : AuditRecord(CreatedBy, UpdatedBy, CreatedUtc, UpdatedUtc, DeletedUtc)
{
    public BarcodeRecord() : this(
        Guid.Empty,
        string.Empty,
        string.Empty,
        string.Empty,
        Guid.Empty,
        Guid.Empty,
        Guid.Empty,
        string.Empty,
        string.Empty,
        DateTime.MinValue,
        DateTime.MinValue,
        null)
    {
    }
}