using FluentValidation;

namespace Pondrop.Service.Product.Application.Queries;

public class GetCategoryByIdQueryHandlerValidator : AbstractValidator<GetCategoryByIdQuery>
{
    public GetCategoryByIdQueryHandlerValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}