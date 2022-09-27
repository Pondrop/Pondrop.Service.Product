using FluentValidation;
using Pondrop.Service.Product.Application.Interfaces.Services;

namespace Pondrop.Service.Product.Application.Commands;

public class CreateProductCommandHandlerValidator : AbstractValidator<CreateProductCommand>
{
    private readonly IAddressService _addressService;
    
    public CreateProductCommandHandlerValidator(IAddressService addressService)
    {
        _addressService = addressService;
        
        RuleFor(x => x.Name).NotEmpty();
    }
}