using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using Pondrop.Service.Product.Application.Interfaces;
using Pondrop.Service.Product.Application.Models;
using Pondrop.Service.Product.Domain.Models;

namespace Pondrop.Service.Product.Application.Queries;

public class GetBrandByIdQueryHandler : IRequestHandler<GetBrandByIdQuery, Result<BrandEntity?>>
{
    private readonly ICheckpointRepository<BrandEntity> _viewRepository;
    private readonly IValidator<GetBrandByIdQuery> _validator;
    private readonly ILogger<GetBrandByIdQueryHandler> _logger;

    public GetBrandByIdQueryHandler(
        ICheckpointRepository<BrandEntity> viewRepository,
        IValidator<GetBrandByIdQuery> validator,
        ILogger<GetBrandByIdQueryHandler> logger)
    {
        _viewRepository = viewRepository;
        _validator = validator;
        _logger = logger;
    }

    public async Task<Result<BrandEntity?>> Handle(GetBrandByIdQuery query, CancellationToken cancellationToken)
    {
        var validation = _validator.Validate(query);

        if (!validation.IsValid)
        {
            var errorMessage = $"Get brand by id failed {validation}";
            _logger.LogError(errorMessage);
            return Result<BrandEntity?>.Error(errorMessage);
        }

        var result = default(Result<BrandEntity?>);

        try
        {
            var record = await _viewRepository.GetByIdAsync(query.Id);

            result = record is not null
                ? Result<BrandEntity?>.Success(record)
                : Result<BrandEntity?>.Success(null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            result = Result<BrandEntity?>.Error(ex);
        }

        return result;
    }
}