using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using Pondrop.Service.Interfaces;
using Pondrop.Service.Product.Application.Models;
using Pondrop.Service.Product.Domain.Models;

namespace Pondrop.Service.Product.Application.Queries;

public class GetCategoryByIdQueryHandler : IRequestHandler<GetCategoryByIdQuery, Result<CategoryEntity?>>
{
    private readonly ICheckpointRepository<CategoryEntity> _viewRepository;
    private readonly IValidator<GetCategoryByIdQuery> _validator;
    private readonly ILogger<GetCategoryByIdQueryHandler> _logger;

    public GetCategoryByIdQueryHandler(
        ICheckpointRepository<CategoryEntity> viewRepository,
        IValidator<GetCategoryByIdQuery> validator,
        ILogger<GetCategoryByIdQueryHandler> logger)
    {
        _viewRepository = viewRepository;
        _validator = validator;
        _logger = logger;
    }

    public async Task<Result<CategoryEntity?>> Handle(GetCategoryByIdQuery query, CancellationToken cancellationToken)
    {
        var validation = _validator.Validate(query);

        if (!validation.IsValid)
        {
            var errorMessage = $"Get category by id failed {validation}";
            _logger.LogError(errorMessage);
            return Result<CategoryEntity?>.Error(errorMessage);
        }

        var result = default(Result<CategoryEntity?>);

        try
        {
            var record = await _viewRepository.GetByIdAsync(query.Id);

            result = record is not null
                ? Result<CategoryEntity?>.Success(record)
                : Result<CategoryEntity?>.Success(null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            result = Result<CategoryEntity?>.Error(ex);
        }

        return result;
    }
}