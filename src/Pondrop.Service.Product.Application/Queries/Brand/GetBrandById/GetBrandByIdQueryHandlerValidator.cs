using FluentValidation;

namespace Pondrop.Service.Product.Application.Queries;

public class GetBrandByIdQueryHandlerValidator : AbstractValidator<GetBrandByIdQuery>
{
    public GetBrandByIdQueryHandlerValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}