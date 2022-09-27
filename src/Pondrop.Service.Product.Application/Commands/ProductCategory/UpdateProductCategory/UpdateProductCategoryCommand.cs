﻿using MediatR;
using Pondrop.Service.Product.Application.Models;
using Pondrop.Service.Product.Domain.Models;
using Pondrop.Service.ProductCategory.Domain.Models;

namespace Pondrop.Service.Product.Application.Commands;

public class UpdateProductCategoryCommand : IRequest<Result<ProductCategoryRecord>>
{
    public Guid Id { get; init; } = Guid.Empty;

    public Guid? ProductId { get; init; } = null;
    public Guid? CategoryId { get; init; } = null;

    public string? PublicationLifecycleId { get; init; } = null;
}