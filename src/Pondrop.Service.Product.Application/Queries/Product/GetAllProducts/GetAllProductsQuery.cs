using MediatR;
using Pondrop.Service.Product.Application.Models;
using Pondrop.Service.Product.Domain.Models;
using Pondrop.Service.Product.Domain.Models.Product;

namespace Pondrop.Service.Product.Application.Queries;

public class GetAllProductsQuery : IRequest<Result<List<ProductEntity>>>
{
    public int Limit { get; set; }

    public int Offset { get; set; }
}