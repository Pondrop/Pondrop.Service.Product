using FluentValidation;

namespace Pondrop.Service.Product.Application.Commands;

public class UpdateProductCommandHandlerValidator : AbstractValidator<UpdateProductCommand>
{
    public UpdateProductCommandHandlerValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}