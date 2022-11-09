using Pondrop.Service.Events;
using Pondrop.Service.Product.Domain.Models;

namespace Pondrop.Service.Product.Domain.Events.ProductCategory;

public record DeleteProductCategory(
    Guid Id) : EventPayload;
