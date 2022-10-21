using FluentValidation;

namespace Pondrop.Service.Product.Application.Queries;

public class GetAllParentCategoriesQueryHandlerValidator : AbstractValidator<GetAllParentCategoriesQuery>
{
    public GetAllParentCategoriesQueryHandlerValidator()
    {
    }
}