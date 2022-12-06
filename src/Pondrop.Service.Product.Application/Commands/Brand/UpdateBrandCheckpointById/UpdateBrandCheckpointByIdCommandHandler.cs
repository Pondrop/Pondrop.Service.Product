using AutoMapper;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Pondrop.Service.Interfaces;
using Pondrop.Service.Product.Domain.Models;

namespace Pondrop.Service.Product.Application.Commands;

public class UpdateBrandCheckpointByIdCommandHandler : UpdateCheckpointByIdCommandHandler<UpdateBrandCheckpointByIdCommand, BrandEntity, BrandRecord>
{
    public UpdateBrandCheckpointByIdCommandHandler(
        IEventRepository eventRepository,
        ICheckpointRepository<BrandEntity> brandCheckpointRepository,
        IMapper mapper,
        IValidator<UpdateCheckpointByIdCommand> validator,
        ILogger<UpdateBrandCheckpointByIdCommandHandler> logger) : base(eventRepository, brandCheckpointRepository, mapper, validator, logger)
    {
    }
}