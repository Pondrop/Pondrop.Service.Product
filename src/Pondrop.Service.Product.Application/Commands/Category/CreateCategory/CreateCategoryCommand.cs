﻿using MediatR;
using Pondrop.Service.Product.Application.Models;
using Pondrop.Service.Product.Domain.Models;

namespace Pondrop.Service.Product.Application.Commands;

public class CreateCategoryCommand : IRequest<Result<CategoryRecord>>
{
    public string Name { get; init; } = string.Empty;
    public string Type { get; init; } = string.Empty;
    public string PublicationLifecycleId { get; init; } = string.Empty;
}
