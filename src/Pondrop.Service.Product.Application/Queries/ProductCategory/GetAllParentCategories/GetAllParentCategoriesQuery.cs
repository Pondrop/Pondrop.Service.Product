using MediatR;
using Pondrop.Service.Product.Application.Models;
using Pondrop.Service.Product.Domain.Models.ProductCategory;

namespace Pondrop.Service.Product.Application.Queries;

public class GetAllParentCategoriesQuery : IRequest<Result<List<ParentCategoryViewRecord>>>
{
    public int Limit { get; set; }
    public int Offset { get; set; }
}