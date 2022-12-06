using FluentValidation;

namespace Pondrop.Service.Product.Application.Queries;

public class GetFullProductByNameQueryHandlerValidator : AbstractValidator<GetProductByNameQuery>
{
    public GetFullProductByNameQueryHandlerValidator()
    {
        RuleFor(x => x.Name).NotEmpty();
    }
}