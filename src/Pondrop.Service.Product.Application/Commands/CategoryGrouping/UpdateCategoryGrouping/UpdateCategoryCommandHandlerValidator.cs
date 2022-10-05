using FluentValidation;
using Pondrop.Service.Product.Application.Interfaces.Services;

namespace Pondrop.Service.Product.Application.Commands;

public class UpdateCategoryGroupingCommandHandlerValidator : AbstractValidator<UpdateCategoryGroupingCommand>
{
    public UpdateCategoryGroupingCommandHandlerValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}