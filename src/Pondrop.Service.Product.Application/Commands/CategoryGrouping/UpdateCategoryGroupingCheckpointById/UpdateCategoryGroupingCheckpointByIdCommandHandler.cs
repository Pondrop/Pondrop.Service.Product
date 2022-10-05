using AutoMapper;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Pondrop.Service.Product.Application.Interfaces;
using Pondrop.Service.Product.Domain.Models;

namespace Pondrop.Service.Product.Application.Commands;

public class UpdateCategoryGroupingCheckpointByIdCommandHandler : UpdateCheckpointByIdCommandHandler<UpdateCategoryGroupingCheckpointByIdCommand, CategoryGroupingEntity, CategoryGroupingRecord>
{
    public UpdateCategoryGroupingCheckpointByIdCommandHandler(
        IEventRepository eventRepository,
        ICheckpointRepository<CategoryGroupingEntity> categoryCheckpointRepository,
        IMapper mapper,
        IValidator<UpdateCheckpointByIdCommand> validator,
        ILogger<UpdateCategoryGroupingCheckpointByIdCommandHandler> logger) : base(eventRepository, categoryCheckpointRepository, mapper, validator, logger)
    {
    }
}