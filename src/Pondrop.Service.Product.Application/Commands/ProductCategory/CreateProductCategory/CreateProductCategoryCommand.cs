using MediatR;
using Pondrop.Service.Product.Application.Models;
using Pondrop.Service.Product.Domain.Models;
using Pondrop.Service.ProductCategory.Domain.Models;

namespace Pondrop.Service.Product.Application.Commands;

public class CreateProductCategoryCommand : IRequest<Result<ProductCategoryRecord>>
{
    public Guid? ProductId { get; init; } = null;
    public Guid? CategoryId { get; init; } = null;

    public string PublicationLifecycleId { get; init; } = string.Empty;
}
