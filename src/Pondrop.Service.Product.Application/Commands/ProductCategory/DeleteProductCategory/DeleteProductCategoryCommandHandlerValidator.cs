using FluentValidation;

namespace Pondrop.Service.Product.Application.Commands;

public class DeleteProductCategoryCommandHandlerValidator : AbstractValidator<DeleteProductCategoryCommand>
{
    public DeleteProductCategoryCommandHandlerValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}