﻿using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using Pondrop.Service.Product.Application.Interfaces;
using Pondrop.Service.Product.Application.Models;
using Pondrop.Service.Product.Domain.Models;

namespace Pondrop.Service.Product.Application.Queries;

public class GetAllCategoryGroupingsQueryHandler : IRequestHandler<GetAllCategoryGroupingsQuery, Result<List<CategoryGroupingViewRecord>>>
{
    private readonly IContainerRepository<CategoryGroupingViewRecord> _categoryGroupingContainerRepository;
    private readonly IMapper _mapper;
    private readonly IValidator<GetAllCategoryGroupingsQuery> _validator;
    private readonly ILogger<GetAllCategoryGroupingsQueryHandler> _logger;

    public GetAllCategoryGroupingsQueryHandler(
        IContainerRepository<CategoryGroupingViewRecord> categoryGroupingContainerRepository,
        IMapper mapper,
        IValidator<GetAllCategoryGroupingsQuery> validator,
        ILogger<GetAllCategoryGroupingsQueryHandler> logger)
    {
        _categoryGroupingContainerRepository = categoryGroupingContainerRepository;
        _mapper = mapper;
        _validator = validator;
        _logger = logger;
    }

    public async Task<Result<List<CategoryGroupingViewRecord>>> Handle(GetAllCategoryGroupingsQuery request, CancellationToken cancellationToken)
    {
        var validation = _validator.Validate(request);

        if (!validation.IsValid)
        {
            var errorMessage = $"Get all categorys failed {validation}";
            _logger.LogError(errorMessage);
            return Result<List<CategoryGroupingViewRecord>>.Error(errorMessage);
        }

        var result = default(Result<List<CategoryGroupingViewRecord>>);

        try
        {
            var records = await _categoryGroupingContainerRepository.GetAllAsync();
            result = Result<List<CategoryGroupingViewRecord>>.Success(records);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            result = Result<List<CategoryGroupingViewRecord>>.Error(ex);
        }

        return result;
    }
}