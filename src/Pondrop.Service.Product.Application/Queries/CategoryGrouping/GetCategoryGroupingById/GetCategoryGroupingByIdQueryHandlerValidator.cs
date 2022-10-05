using FluentValidation;

namespace Pondrop.Service.Product.Application.Queries;

public class GetCategoryGroupingByIdQueryHandlerValidator : AbstractValidator<GetCategoryGroupingByIdQuery>
{
    public GetCategoryGroupingByIdQueryHandlerValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}