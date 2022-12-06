using FluentValidation;

namespace Pondrop.Service.Product.Application.Commands;

public class UpdateBrandCommandHandlerValidator : AbstractValidator<UpdateBrandCommand>
{
    public UpdateBrandCommandHandlerValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}