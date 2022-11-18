using MediatR;
using Pondrop.Service.Product.Application.Models;
using Pondrop.Service.Product.Domain.Models;
using Pondrop.Service.ProductCategory.Domain.Models;

namespace Pondrop.Service.Product.Application.Commands;

public class SetProductsCommand : IRequest<Result<List<ProductCategoryRecord>>>
{
    public Guid CategoryId { get; init; } = Guid.Empty;
    public List<Guid>? ProductIds { get; init; } = null;
    public string PublicationLifecycleId { get; init; } = String.Empty;
}