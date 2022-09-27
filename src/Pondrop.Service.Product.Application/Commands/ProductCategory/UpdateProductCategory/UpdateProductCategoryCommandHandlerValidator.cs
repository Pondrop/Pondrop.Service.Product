using FluentValidation;

namespace Pondrop.Service.Product.Application.Commands;

public class UpdateProductCategoryCommandHandlerValidator : AbstractValidator<UpdateProductCategoryCommand>
{
    public UpdateProductCategoryCommandHandlerValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}