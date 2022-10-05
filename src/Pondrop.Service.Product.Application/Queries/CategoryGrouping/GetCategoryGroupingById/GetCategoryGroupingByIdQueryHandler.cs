using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using Pondrop.Service.Product.Application.Interfaces;
using Pondrop.Service.Product.Application.Models;
using Pondrop.Service.Product.Domain.Models;

namespace Pondrop.Service.Product.Application.Queries;

public class GetCategoryGroupingByIdQueryHandler : IRequestHandler<GetCategoryGroupingByIdQuery, Result<CategoryGroupingEntity?>>
{
    private readonly ICheckpointRepository<CategoryGroupingEntity> _checkpointRepository;
    private readonly IValidator<GetCategoryGroupingByIdQuery> _validator;
    private readonly ILogger<GetCategoryGroupingByIdQueryHandler> _logger;

    public GetCategoryGroupingByIdQueryHandler(
        ICheckpointRepository<CategoryGroupingEntity> checkpointRepository,
        IValidator<GetCategoryGroupingByIdQuery> validator,
        ILogger<GetCategoryGroupingByIdQueryHandler> logger)
    {
        _checkpointRepository = checkpointRepository;
        _validator = validator;
        _logger = logger;
    }

    public async Task<Result<CategoryGroupingEntity?>> Handle(GetCategoryGroupingByIdQuery query, CancellationToken cancellationToken)
    {
        var validation = _validator.Validate(query);

        if (!validation.IsValid)
        {
            var errorMessage = $"Get category by id failed {validation}";
            _logger.LogError(errorMessage);
            return Result<CategoryGroupingEntity?>.Error(errorMessage);
        }

        var result = default(Result<CategoryGroupingEntity?>);

        try
        {
            var record = await _checkpointRepository.GetByIdAsync(query.Id);

            result = record is not null
                ? Result<CategoryGroupingEntity?>.Success(record)
                : Result<CategoryGroupingEntity?>.Success(null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            result = Result<CategoryGroupingEntity?>.Error(ex);
        }

        return result;
    }
}