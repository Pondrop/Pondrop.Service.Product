using FluentValidation;

namespace Pondrop.Service.Product.Application.Commands;

public class SetProductCategoriesCommandHandlerValidator : AbstractValidator<SetProductCategoriesCommand>
{
    public SetProductCategoriesCommandHandlerValidator()
    {
        RuleFor(x => x.ProductId).NotEmpty();
        RuleFor(x => x.CategoryIds).NotNull();
    }
}