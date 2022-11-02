using Pondrop.Service.Product.Domain.Models;

namespace Pondrop.Service.Product.Domain.Events.ProductCategory;

public record UpdateProductCategory(
    string? PublicationLifecycleId) : EventPayload;
