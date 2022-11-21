using Pondrop.Service.Events;
using Pondrop.Service.Product.Domain.Models;

namespace Pondrop.Service.Product.Domain.Events.Category;

public record UpdateCategory(
    string? Name,
    string? Type,
    string? PublicationLifecycleId) : EventPayload;
