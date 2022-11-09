using FluentValidation;
using Pondrop.Service.Interfaces.Services;

namespace Pondrop.Service.Product.Application.Commands;

public class CreateProductCategoryCommandHandlerValidator : AbstractValidator<CreateProductCategoryCommand>
{
    private readonly IAddressService _addressService;
    
    public CreateProductCategoryCommandHandlerValidator(IAddressService addressService)
    {
        _addressService = addressService;
        
        RuleFor(x => x.ProductId).NotEmpty();
        RuleFor(x => x.CategoryId).NotEmpty();
    }
}