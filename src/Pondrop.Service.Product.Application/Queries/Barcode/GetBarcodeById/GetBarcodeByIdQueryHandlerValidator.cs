using FluentValidation;

namespace Pondrop.Service.Product.Application.Queries;

public class GetBarcodeByIdQueryHandlerValidator : AbstractValidator<GetBarcodeByIdQuery>
{
    public GetBarcodeByIdQueryHandlerValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}