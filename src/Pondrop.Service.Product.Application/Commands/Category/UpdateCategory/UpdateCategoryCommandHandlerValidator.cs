using FluentValidation;

namespace Pondrop.Service.Product.Application.Commands;

public class UpdateCategoryCommandHandlerValidator : AbstractValidator<UpdateCategoryCommand>
{
    public UpdateCategoryCommandHandlerValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}