using FluentValidation;
using Pondrop.Service.Product.Application.Interfaces.Services;

namespace Pondrop.Service.Product.Application.Commands;

public class UpdateCategoryCommandHandlerValidator : AbstractValidator<UpdateCategoryCommand>
{
    public UpdateCategoryCommandHandlerValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}