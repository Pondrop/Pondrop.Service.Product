﻿using AutoMapper;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Pondrop.Service.Interfaces;
using Pondrop.Service.Product.Domain.Models;

namespace Pondrop.Service.Product.Application.Commands;

public class UpdateCategoryCheckpointByIdCommandHandler : UpdateCheckpointByIdCommandHandler<UpdateCategoryCheckpointByIdCommand, CategoryEntity, CategoryRecord>
{
    public UpdateCategoryCheckpointByIdCommandHandler(
        IEventRepository eventRepository,
        ICheckpointRepository<CategoryEntity> categoryCheckpointRepository,
        IMapper mapper,
        IValidator<UpdateCheckpointByIdCommand> validator,
        ILogger<UpdateCategoryCheckpointByIdCommandHandler> logger) : base(eventRepository, categoryCheckpointRepository, mapper, validator, logger)
    {
    }
}