using FluentValidation;

namespace Pondrop.Service.Product.Application.Queries;

public class GetProductByIdQueryHandlerValidator : AbstractValidator<GetProductByIdQuery>
{
    public GetProductByIdQueryHandlerValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}