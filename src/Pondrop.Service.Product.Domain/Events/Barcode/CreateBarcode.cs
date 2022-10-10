using Pondrop.Service.Product.Domain.Models;

namespace Pondrop.Service.Product.Domain.Events.Barcode;

public record CreateBarcode(
    Guid Id,
    string BarcodeNumber,
    string BarcodeText,
    string BarcodeType,
    Guid ProductId,
    Guid RetailerId,
    Guid CompanyId,
    string PublicationLifecycleId) : EventPayload;
