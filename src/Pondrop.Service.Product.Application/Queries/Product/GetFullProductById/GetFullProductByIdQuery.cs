using MediatR;
using Pondrop.Service.Product.Application.Models;
using Pondrop.Service.Product.Domain.Models;
using Pondrop.Service.Product.Domain.Models.Product;

namespace Pondrop.Service.Product.Application.Queries;

public class GetFullProductByIdQuery : IRequest<Result<ProductViewRecord?>>
{
    public Guid Id { get; init; } = Guid.Empty;
}