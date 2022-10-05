using Pondrop.Service.Product.Domain.Models;

namespace Pondrop.Service.Product.Domain.Events.CategoryGrouping;

public record CreateCategoryGrouping(
    Guid Id,
    Guid HigherLevelCategoryId,
    Guid LowerLevelCategoryId,
    string? Description,
    string? PublicationLifecycleId) : EventPayload;
