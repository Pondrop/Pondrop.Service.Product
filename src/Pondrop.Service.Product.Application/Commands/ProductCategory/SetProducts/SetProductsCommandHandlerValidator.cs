using FluentValidation;

namespace Pondrop.Service.Product.Application.Commands;

public class SetProductsCommandHandlerValidator : AbstractValidator<SetProductsCommand>
{
    public SetProductsCommandHandlerValidator()
    {
        RuleFor(x => x.ProductIds).NotNull();
        RuleFor(x => x.CategoryId).NotEmpty();
    }
}