using MediatR;
using Pondrop.Service.Product.Application.Models;
using Pondrop.Service.Product.Domain.Models;

namespace Pondrop.Service.Product.Application.Commands;

public class UpdateCategoryCommand : IRequest<Result<CategoryRecord>>
{
    public Guid Id { get; init; } = Guid.Empty;
    public string? Name { get; init; } = null;
    public string? Type { get; init; } = null;
    public string? PublicationLifecycleId { get; init; } = null;
}