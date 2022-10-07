using Pondrop.Service.Product.Domain.Models;

namespace Pondrop.Service.Product.Domain.Events.Brand;

public record CreateBrand(
    Guid Id,
    string Name,
    Guid CompanyId,
    string PublicationLifecycleId) : EventPayload;
