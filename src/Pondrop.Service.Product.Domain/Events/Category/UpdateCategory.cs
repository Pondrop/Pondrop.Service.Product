using Pondrop.Service.Product.Domain.Models;

namespace Pondrop.Service.Product.Domain.Events.Category;

public record UpdateCategory(
    string? CategoryName,
    string? Description,
    string? PublicationLifecycleId) : EventPayload;
