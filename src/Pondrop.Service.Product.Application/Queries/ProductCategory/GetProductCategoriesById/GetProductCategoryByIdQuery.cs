using MediatR;
using Pondrop.Service.Product.Application.Models;
using Pondrop.Service.ProductCategory.Domain.Models;

namespace Pondrop.Service.Product.Application.Queries;

public class GetProductCategoryByIdQuery : IRequest<Result<ProductCategoryEntity?>>
{
    public Guid Id { get; init; } = Guid.Empty;
}