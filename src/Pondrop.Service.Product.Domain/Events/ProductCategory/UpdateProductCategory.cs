using Pondrop.Service.Product.Domain.Models;

namespace Pondrop.Service.Product.Domain.Events.ProductCategory;

public record UpdateProductCategory(
    Guid? CategoryId,
    Guid? ProductId,
    string? PublicationLifecycleId) : EventPayload;
