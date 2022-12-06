using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using Pondrop.Service.Interfaces;
using Pondrop.Service.Product.Application.Models;
using Pondrop.Service.Product.Domain.Models.Product;

namespace Pondrop.Service.Product.Application.Queries;

public class GetProductByNameQueryHandler : IRequestHandler<GetProductByNameQuery, Result<ProductEntity?>>
{
    private readonly ICheckpointRepository<ProductEntity> _viewRepository;
    private readonly IValidator<GetProductByNameQuery> _validator;
    private readonly ILogger<GetProductByNameQueryHandler> _logger;

    public GetProductByNameQueryHandler(
        ICheckpointRepository<ProductEntity> viewRepository,
        IValidator<GetProductByNameQuery> validator,
        ILogger<GetProductByNameQueryHandler> logger)
    {
        _viewRepository = viewRepository;
        _validator = validator;
        _logger = logger;
    }

    public async Task<Result<ProductEntity?>> Handle(GetProductByNameQuery query, CancellationToken cancellationToken)
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
            var record = await _viewRepository.QueryAsync($"SELECT * FROM c WHERE c.name = '{query.Name}'");

            result = record is not null
                ? Result<ProductEntity?>.Success(record?.FirstOrDefault())
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