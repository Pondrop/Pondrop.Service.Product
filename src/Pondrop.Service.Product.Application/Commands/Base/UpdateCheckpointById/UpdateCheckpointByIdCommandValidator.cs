using FluentValidation;

namespace Pondrop.Service.Product.Application.Commands;

public class UpdateCheckpointByIdCommandValidator : AbstractValidator<UpdateCheckpointByIdCommand>
{
    public UpdateCheckpointByIdCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}