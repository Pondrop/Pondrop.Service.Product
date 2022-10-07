using FluentValidation;

namespace Pondrop.Service.Product.Application.Queries;

public class GetAllCategoryGroupingsQueryHandlerValidator : AbstractValidator<GetAllCategoryGroupingsQuery>
{
    public GetAllCategoryGroupingsQueryHandlerValidator()
    {
    }
}