﻿using MediatR;
using Pondrop.Service.Product.Application.Models;
using Pondrop.Service.Product.Domain.Models;

namespace Pondrop.Service.Product.Application.Queries;

public class GetBrandByIdQuery : IRequest<Result<BrandEntity?>>
{
    public Guid Id { get; init; } = Guid.Empty;
}