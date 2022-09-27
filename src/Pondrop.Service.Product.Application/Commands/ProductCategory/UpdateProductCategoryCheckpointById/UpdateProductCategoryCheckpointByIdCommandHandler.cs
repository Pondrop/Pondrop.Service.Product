using AutoMapper;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Pondrop.Service.Product.Application.Commands;
using Pondrop.Service.Product.Application.Interfaces;
using Pondrop.Service.Product.Domain.Models;
using Pondrop.Service.ProductCategory.Domain.Models;

namespace Pondrop.Service.Product.Application.Commands;

public class UpdateProductCategoryCheckpointByIdCommandHandler : UpdateCheckpointByIdCommandHandler<UpdateProductCategoryCheckpointByIdCommand, ProductCategoryEntity, ProductCategoryRecord>
{
    public UpdateProductCategoryCheckpointByIdCommandHandler(
        IEventRepository eventRepository,
        ICheckpointRepository<ProductCategoryEntity> ProductCategoryCheckpointRepository,
        IMapper mapper,
        IValidator<UpdateCheckpointByIdCommand> validator,
        ILogger<UpdateProductCategoryCheckpointByIdCommandHandler> logger) : base(eventRepository, ProductCategoryCheckpointRepository, mapper, validator, logger)
    {
    }
}