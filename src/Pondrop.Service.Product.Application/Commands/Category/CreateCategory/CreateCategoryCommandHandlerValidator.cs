using FluentValidation;
using Pondrop.Service.Product.Application.Interfaces.Services;

namespace Pondrop.Service.Product.Application.Commands;

public class CreateCategoryCommandHandlerValidator : AbstractValidator<CreateCategoryCommand>
{
    private readonly IAddressService _addressService;
    
    public CreateCategoryCommandHandlerValidator(IAddressService addressService)
    {
        _addressService = addressService;
        
        RuleFor(x => x.CategoryName).NotEmpty();
        RuleFor(x => x.Description).NotEmpty();
        RuleFor(x => x.PublicationLifecycleId).NotEmpty();
    }
}