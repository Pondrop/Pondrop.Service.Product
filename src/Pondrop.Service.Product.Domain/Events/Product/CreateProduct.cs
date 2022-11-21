using Pondrop.Service.Events;
using Pondrop.Service.Product.Domain.Models;

namespace Pondrop.Service.Product.Domain.Events.Product;

public record CreateProduct(
    Guid Id,
    string Name,
    Guid BrandId,
    string ExternalReferenceId,
    string Variant,
    string AltName,
    string ShortDescription,
    double NetContent,
    string NetContentUom,
    string PossibleCategories,
    string PublicationLifecycleId,
    List<Guid> ChildProductId) : EventPayload;
