using MediatR;
using Pondrop.Service.Product.Application.Models;
using Pondrop.Service.Product.Domain.Models;

namespace Pondrop.Service.Product.Application.Queries;

public class GetCategoryByIdQuery : IRequest<Result<CategoryEntity?>>
{
    public Guid Id { get; init; } = Guid.Empty;
}