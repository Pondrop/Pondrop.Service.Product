﻿using AutoMapper;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Pondrop.Service.Product.Application.Commands;
using Pondrop.Service.Product.Application.Interfaces;
using Pondrop.Service.Product.Application.Interfaces.Services;
using Pondrop.Service.Product.Application.Models;
using Pondrop.Service.Product.Domain.Models;
using Pondrop.Service.ProductCategory.Domain.Models;

namespace Pondrop.Service.Product.Application.Commands;

public class CreateProductCategoryCommandHandler : DirtyCommandHandler<ProductCategoryEntity, CreateProductCategoryCommand, Result<ProductCategoryRecord>>
{
    private readonly IEventRepository _eventRepository;
    private readonly IMapper _mapper;
    private readonly IUserService _userService;
    private readonly IValidator<CreateProductCategoryCommand> _validator;
    private readonly ILogger<CreateProductCategoryCommandHandler> _logger;

    public CreateProductCategoryCommandHandler(
        IOptions<ProductCategoryUpdateConfiguration> ProductCategoryUpdateConfig,
        IEventRepository eventRepository,
        IDaprService daprService,
        IUserService userService,
        IMapper mapper,
        IValidator<CreateProductCategoryCommand> validator,
        ILogger<CreateProductCategoryCommandHandler> logger) : base(eventRepository, ProductCategoryUpdateConfig.Value, daprService, logger)
    {
        _eventRepository = eventRepository;
        _mapper = mapper;
        _userService = userService;
        _validator = validator;
        _logger = logger;
    }

    public override async Task<Result<ProductCategoryRecord>> Handle(CreateProductCategoryCommand command, CancellationToken cancellationToken)
    {
        var validation = _validator.Validate(command);

        if (!validation.IsValid)
        {
            var errorMessage = $"Create ProductCategory failed, errors on validation {validation}";
            _logger.LogError(errorMessage);
            return Result<ProductCategoryRecord>.Error(errorMessage);
        }

        var result = default(Result<ProductCategoryRecord>);

        try
        {
            var ProductCategoryEntity = new ProductCategoryEntity(
                command.CategoryId ?? Guid.Empty,
                command.ProductId ?? Guid.Empty,
                command.PublicationLifecycleId,
                _userService.CurrentUserId());

            var success = await _eventRepository.AppendEventsAsync(ProductCategoryEntity.StreamId, 0, ProductCategoryEntity.GetEvents());

            await Task.WhenAll(
                InvokeDaprMethods(ProductCategoryEntity.Id, ProductCategoryEntity.GetEvents()));

            result = success
                ? Result<ProductCategoryRecord>.Success(_mapper.Map<ProductCategoryRecord>(ProductCategoryEntity))
                : Result<ProductCategoryRecord>.Error(FailedToCreateMessage(command));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, FailedToCreateMessage(command));
            result = Result<ProductCategoryRecord>.Error(ex);
        }

        return result;
    }

    private static string FailedToCreateMessage(CreateProductCategoryCommand command) =>
        $"Failed to create ProductCategory\nCommand: '{JsonConvert.SerializeObject(command)}'";
}