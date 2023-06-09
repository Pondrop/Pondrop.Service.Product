﻿using FluentValidation;
using Pondrop.Service.Interfaces.Services;

namespace Pondrop.Service.Product.Application.Commands;

public class CreateBrandCommandHandlerValidator : AbstractValidator<CreateBrandCommand>
{
    private readonly IAddressService _addressService;
    
    public CreateBrandCommandHandlerValidator(IAddressService addressService)
    {
        _addressService = addressService;
        
        RuleFor(x => x.Name).NotEmpty();
        RuleFor(x => x.CompanyId).NotEmpty();
    }
}