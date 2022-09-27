using AutoMapper;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Pondrop.Service.Product.Application.Interfaces;
using Pondrop.Service.Product.Domain.Models;
using Pondrop.Service.Product.Domain.Models.Product;

namespace Pondrop.Service.Product.Application.Commands;

public class UpdateProductCheckpointByIdCommandHandler : UpdateCheckpointByIdCommandHandler<UpdateProductCheckpointByIdCommand, ProductEntity, ProductRecord>
{
    public UpdateProductCheckpointByIdCommandHandler(
        IEventRepository eventRepository,
        ICheckpointRepository<ProductEntity> productCheckpointRepository,
        IMapper mapper,
        IValidator<UpdateCheckpointByIdCommand> validator,
        ILogger<UpdateProductCheckpointByIdCommandHandler> logger) : base(eventRepository, productCheckpointRepository, mapper, validator, logger)
    {
    }
}