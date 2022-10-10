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
    DateTime UpdatedUtc)
    : AuditRecord(CreatedBy, UpdatedBy, CreatedUtc, UpdatedUtc)
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
        DateTime.MinValue)
    {
    }
}