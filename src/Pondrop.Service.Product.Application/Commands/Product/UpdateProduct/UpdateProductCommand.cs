using MediatR;
using Pondrop.Service.Product.Application.Models;
using Pondrop.Service.Product.Domain.Models;
using Pondrop.Service.Product.Domain.Models.Product;

namespace Pondrop.Service.Product.Application.Commands;

public class UpdateProductCommand : IRequest<Result<ProductRecord>>
{
    public Guid Id { get; init; } = Guid.Empty;

    public string? Name { get; init; } = null;

    public Guid? BrandId { get; init; } = null;

    public string? ExternalReferenceId { get; init; } = null;
    public string? Variant { get; init; } = null;

    public string? AltName { get; init; } = null;

    public string? ShortDescription { get; init; } = null;

    public double NetContent { get; init; } = 0;

    public string? NetContentUom { get; init; } = null;

    public string? PossibleCategories { get; init; } = null;

    public List<Guid>? ChildProductId { get; init; } = null;
    public string? PublicationLifecycleId { get; init; } = null;
}