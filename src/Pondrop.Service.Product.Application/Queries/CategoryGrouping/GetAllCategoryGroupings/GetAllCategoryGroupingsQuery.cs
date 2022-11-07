using MediatR;
using Pondrop.Service.Product.Application.Models;
using Pondrop.Service.Product.Domain.Models;

namespace Pondrop.Service.Product.Application.Queries;

public class GetAllCategoryGroupingsQuery : IRequest<Result<List<CategoryGroupingViewRecord>>>
{
    public int Limit { get; set; }

    public int Offset { get; set; }
}