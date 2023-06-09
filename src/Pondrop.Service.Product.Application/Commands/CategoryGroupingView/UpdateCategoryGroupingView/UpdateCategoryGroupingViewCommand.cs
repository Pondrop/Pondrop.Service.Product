﻿using MediatR;
using Pondrop.Service.Product.Application.Models;
using Pondrop.Service.Product.Domain.Models;

namespace Pondrop.Service.Product.Application.Commands;

public class UpdateCategoryGroupingViewCommand : IRequest<Result<int>>
{
    public Guid? CategoryGroupingId { get; init; } = null;
    public Guid? CategoryId { get; init; } = null;
}