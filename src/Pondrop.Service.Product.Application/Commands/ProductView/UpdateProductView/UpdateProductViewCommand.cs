using MediatR;
using Pondrop.Service.Product.Application.Models;
using Pondrop.Service.Product.Domain.Models;

namespace Pondrop.Service.Product.Application.Commands;

public class UpdateProductViewCommand : IRequest<Result<int>>
{
    public Guid? CategoryId { get; init; } = null;
    public Guid? ProductId { get; init; } = null;
}