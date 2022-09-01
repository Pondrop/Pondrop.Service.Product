using Pondrop.Service.Product.Domain.Models;

namespace Pondrop.Service.Product.Domain.Events.Category;

public record CreateCategory(
    Guid Id,
    string CategoryName,
    string Description,
    string PublicationLifecycleId) : EventPayload;
