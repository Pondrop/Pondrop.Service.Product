﻿using FluentValidation;
using Pondrop.Service.Interfaces.Services;

namespace Pondrop.Service.Product.Application.Commands;

public class CreateCategoryCommandHandlerValidator : AbstractValidator<CreateCategoryCommand>
{
    private readonly IAddressService _addressService;
    
    public CreateCategoryCommandHandlerValidator(IAddressService addressService)
    {
        _addressService = addressService;
        
        RuleFor(x => x.Name).NotEmpty();
    }
}