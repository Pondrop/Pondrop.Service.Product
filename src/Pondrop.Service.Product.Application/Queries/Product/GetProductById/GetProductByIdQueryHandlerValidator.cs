using FluentValidation;

namespace Pondrop.Service.Product.Application.Queries;

public class GetFullProductByIdQueryHandlerValidator : AbstractValidator<GetFullProductByIdQuery>
{
    public GetFullProductByIdQueryHandlerValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}