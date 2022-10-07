using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using Pondrop.Service.Product.Application.Interfaces;
using Pondrop.Service.Product.Application.Models;
using Pondrop.Service.Product.Domain.Models;

namespace Pondrop.Service.Product.Application.Queries;

public class GetCategoryGroupingByIdQueryHandler : IRequestHandler<GetCategoryGroupingByIdQuery, Result<CategoryGroupingViewRecord?>>
{
    private readonly IContainerRepository<CategoryGroupingViewRecord> _containerRepository;
    private readonly IValidator<GetCategoryGroupingByIdQuery> _validator;
    private readonly ILogger<GetCategoryGroupingByIdQueryHandler> _logger;

    public GetCategoryGroupingByIdQueryHandler(
        IContainerRepository<CategoryGroupingViewRecord> containerRepository,
        IValidator<GetCategoryGroupingByIdQuery> validator,
        ILogger<GetCategoryGroupingByIdQueryHandler> logger)
    {
        _containerRepository = containerRepository;
        _validator = validator;
        _logger = logger;
    }

    public async Task<Result<CategoryGroupingViewRecord?>> Handle(GetCategoryGroupingByIdQuery query, CancellationToken cancellationToken)
    {
        var validation = _validator.Validate(query);

        if (!validation.IsValid)
        {
            var errorMessage = $"Get category by id failed {validation}";
            _logger.LogError(errorMessage);
            return Result<CategoryGroupingViewRecord?>.Error(errorMessage);
        }

        var result = default(Result<CategoryGroupingViewRecord?>);

        try
        {
            var record = await _containerRepository.GetByIdAsync(query.Id);

            result = record is not null
                ? Result<CategoryGroupingViewRecord?>.Success(record)
                : Result<CategoryGroupingViewRecord?>.Success(null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            result = Result<CategoryGroupingViewRecord?>.Error(ex);
        }

        return result;
    }
}