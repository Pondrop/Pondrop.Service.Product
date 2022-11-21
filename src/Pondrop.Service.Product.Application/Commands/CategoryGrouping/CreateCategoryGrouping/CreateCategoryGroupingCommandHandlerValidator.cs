using FluentValidation;
using Pondrop.Service.Interfaces.Services;

namespace Pondrop.Service.Product.Application.Commands;

public class CreateCategoryGroupingCommandHandlerValidator : AbstractValidator<CreateCategoryGroupingCommand>
{
    private readonly IAddressService _addressService;
    
    public CreateCategoryGroupingCommandHandlerValidator(IAddressService addressService)
    {
        _addressService = addressService;
        
        RuleFor(x => x.HigherLevelCategoryId).NotEmpty();
        RuleFor(x => x.LowerLevelCategoryId).NotEmpty();
    }
}