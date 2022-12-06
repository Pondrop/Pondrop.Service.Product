using Pondrop.Service.Events;

namespace Pondrop.Service.Product.Domain.Events.Brand;

public record UpdateBrand(
    string? Name,
    Guid? CompanyId,
    string? WebsiteUrl,
    string? Description,
    string? PublicationLifecycleId) : EventPayload;
