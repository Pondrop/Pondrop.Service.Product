using MediatR;
using Pondrop.Service.Product.Application.Models;
using Pondrop.Service.Product.Domain.Models;
using Pondrop.Service.Product.Domain.Models.Product;

namespace Pondrop.Service.Product.Application.Commands;

public class CreateProductWithCategoriesCommand : IRequest<Result<ProductRecord>>
{
    public string Name { get; init; } = string.Empty;

    public Guid? BrandId { get; init; } = null;

    public string ExternalReferenceId { get; init; } = string.Empty;

    public string Variant { get; init; } = string.Empty;

    public string AltName { get; init; } = string.Empty;

    public string ShortDescription { get; init; } = string.Empty;

    public double NetContent { get; init; } = 0;

    public string NetContentUom { get; init; } = string.Empty;

    public string PossibleCategories { get; init; } = string.Empty;

    public List<Guid>? ChildProductId { get; init; } = null;

    public string PublicationLifecycleId { get; init; } = string.Empty;

    public List<Guid>? CategoryIds { get; init; } = null;
    public string BarcodeNumber { get; init; } = String.Empty;
}

