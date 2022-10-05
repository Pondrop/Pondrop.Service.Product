using MediatR;
using Pondrop.Service.Product.Application.Models;
using Pondrop.Service.Product.Domain.Models;

namespace Pondrop.Service.Product.Application.Commands;

public class UpdateCategoryGroupingCommand : IRequest<Result<CategoryGroupingRecord>>
{
    public Guid Id { get; init; } = Guid.Empty;
    public string Description { get; init; } = string.Empty;
    public Guid? HigherLevelCategoryId { get; init; } = null;
    public Guid? LowerLevelCategoryId { get; init; } = null;
    public string PublicationLifecycleId { get; init; } = string.Empty;
}