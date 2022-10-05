using Pondrop.Service.Product.Domain.Models;

namespace Pondrop.Service.Product.Domain.Events.CategoryGrouping;

public record UpdateCategoryGrouping(
    Guid? HigherLevelCategoryId,
    Guid? LowerLevelCategoryId,
    string? Description,
    string? PublicationLifecycleId) : EventPayload;
