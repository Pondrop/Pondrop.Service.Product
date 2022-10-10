using FluentValidation;
using Pondrop.Service.Product.Application.Interfaces.Services;

namespace Pondrop.Service.Product.Application.Commands;

public class CreateBarcodeCommandHandlerValidator : AbstractValidator<CreateBarcodeCommand>
{
    private readonly IAddressService _addressService;
    
    public CreateBarcodeCommandHandlerValidator(IAddressService addressService)
    {
        _addressService = addressService;
        
        RuleFor(x => x.BarcodeNumber).NotEmpty();
    }
}