using FluentValidation;

namespace Pondrop.Service.Product.Application.Queries;

public class GetAllCategoriesQueryHandlerValidator : AbstractValidator<GetAllCategoriesQuery>
{
    public GetAllCategoriesQueryHandlerValidator()
    {
    }
}