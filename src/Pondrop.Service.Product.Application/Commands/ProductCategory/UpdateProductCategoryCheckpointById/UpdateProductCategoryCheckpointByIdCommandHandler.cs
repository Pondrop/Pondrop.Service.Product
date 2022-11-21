using AutoMapper;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Pondrop.Service.Interfaces;
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