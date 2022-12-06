using MediatR;
using Pondrop.Service.Product.Application.Models;
using Pondrop.Service.Product.Domain.Models;
using Pondrop.Service.Product.Domain.Models.Product;

namespace Pondrop.Service.Product.Application.Queries;

public class GetProductByNameQuery : IRequest<Result<ProductEntity?>>
{
    public string Name { get; init; } = string.Empty;
}