using FluentValidation;
using Pondrop.Service.Product.Application.Interfaces.Services;

namespace Pondrop.Service.Product.Application.Commands;

public class UpdateBarcodeCommandHandlerValidator : AbstractValidator<UpdateBarcodeCommand>
{
    public UpdateBarcodeCommandHandlerValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}