﻿using MediatR;
using Pondrop.Service.Product.Application.Models;
using Pondrop.Service.Product.Domain.Models;

namespace Pondrop.Service.Product.Application.Commands;

public class RebuildCategoryViewCommand : IRequest<Result<int>>
{
}