using FluentValidation;

namespace Pondrop.Service.Product.Application.Queries;

public class GetProductCategoryByIdQueryHandlerValidator : AbstractValidator<GetProductCategoryByIdQuery>
{
    public GetProductCategoryByIdQueryHandlerValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}