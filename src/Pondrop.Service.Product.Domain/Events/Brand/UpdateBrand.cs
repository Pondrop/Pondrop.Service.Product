using Pondrop.Service.Product.Domain.Models;

namespace Pondrop.Service.Product.Domain.Events.Brand;

public record UpdateBrand(
    string? Name,
    Guid? CompanyId,
    string? PublicationLifecycleId) : EventPayload;
