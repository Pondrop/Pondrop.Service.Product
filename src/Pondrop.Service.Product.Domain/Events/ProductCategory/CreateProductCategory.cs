using Pondrop.Service.Events;
using Pondrop.Service.Product.Domain.Models;

namespace Pondrop.Service.Product.Domain.Events.ProductCategory;

public record CreateProductCategory(
    Guid Id,
    Guid CategoryId,
    Guid ProductId,
    string PublicationLifecycleId) : EventPayload;
