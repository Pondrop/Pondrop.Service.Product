using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using Pondrop.Service.Product.Application.Interfaces;
using Pondrop.Service.Product.Application.Models;
using Pondrop.Service.ProductCategory.Domain.Models;

namespace Pondrop.Service.Product.Application.Queries;

public class GetProductCategoryByIdQueryHandler : IRequestHandler<GetProductCategoryByIdQuery, Result<ProductCategoryEntity?>>
{
    private readonly ICheckpointRepository<ProductCategoryEntity> _viewRepository;
    private readonly IValidator<GetProductCategoryByIdQuery> _validator;
    private readonly ILogger<GetProductCategoryByIdQueryHandler> _logger;

    public GetProductCategoryByIdQueryHandler(
        ICheckpointRepository<ProductCategoryEntity> viewRepository,
        IValidator<GetProductCategoryByIdQuery> validator,
        ILogger<GetProductCategoryByIdQueryHandler> logger)
    {
        _viewRepository = viewRepository;
        _validator = validator;
        _logger = logger;
    }

    public async Task<Result<ProductCategoryEntity?>> Handle(GetProductCategoryByIdQuery query, CancellationToken cancellationToken)
    {
        var validation = _validator.Validate(query);

        if (!validation.IsValid)
        {
            var errorMessage = $"Get ProductCategory by id failed {validation}";
            _logger.LogError(errorMessage);
            return Result<ProductCategoryEntity?>.Error(errorMessage);
        }

        var result = default(Result<ProductCategoryEntity?>);

        try
        {
            var record = await _viewRepository.GetByIdAsync(query.Id);

            result = record is not null
                ? Result<ProductCategoryEntity?>.Success(record)
                : Result<ProductCategoryEntity?>.Success(null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            result = Result<ProductCategoryEntity?>.Error(ex);
        }

        return result;
    }
}