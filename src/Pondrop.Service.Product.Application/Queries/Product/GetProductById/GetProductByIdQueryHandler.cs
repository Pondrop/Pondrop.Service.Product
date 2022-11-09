using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using Pondrop.Service.Interfaces;
using Pondrop.Service.Product.Application.Models;
using Pondrop.Service.Product.Domain.Models.Product;

namespace Pondrop.Service.Product.Application.Queries;

public class GetProductByIdQueryHandler : IRequestHandler<GetProductByIdQuery, Result<ProductEntity?>>
{
    private readonly ICheckpointRepository<ProductEntity> _viewRepository;
    private readonly IValidator<GetProductByIdQuery> _validator;
    private readonly ILogger<GetProductByIdQueryHandler> _logger;

    public GetProductByIdQueryHandler(
        ICheckpointRepository<ProductEntity> viewRepository,
        IValidator<GetProductByIdQuery> validator,
        ILogger<GetProductByIdQueryHandler> logger)
    {
        _viewRepository = viewRepository;
        _validator = validator;
        _logger = logger;
    }

    public async Task<Result<ProductEntity?>> Handle(GetProductByIdQuery query, CancellationToken cancellationToken)
    {
        var validation = _validator.Validate(query);

        if (!validation.IsValid)
        {
            var errorMessage = $"Get Product by id failed {validation}";
            _logger.LogError(errorMessage);
            return Result<ProductEntity?>.Error(errorMessage);
        }

        var result = default(Result<ProductEntity?>);

        try
        {
            var record = await _viewRepository.GetByIdAsync(query.Id);

            result = record is not null
                ? Result<ProductEntity?>.Success(record)
                : Result<ProductEntity?>.Success(null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            result = Result<ProductEntity?>.Error(ex);
        }

        return result;
    }
}