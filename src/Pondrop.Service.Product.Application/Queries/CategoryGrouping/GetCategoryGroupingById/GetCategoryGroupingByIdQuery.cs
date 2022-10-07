using MediatR;
using Pondrop.Service.Product.Application.Models;
using Pondrop.Service.Product.Domain.Models;

namespace Pondrop.Service.Product.Application.Queries;

public class GetCategoryGroupingByIdQuery : IRequest<Result<CategoryGroupingViewRecord?>>
{
    public Guid Id { get; init; } = Guid.Empty;
}