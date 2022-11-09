using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using Pondrop.Service.Product.Application.Interfaces;
using Pondrop.Service.Product.Application.Models;
using Pondrop.Service.Product.Domain.Models;
using Pondrop.Service.Product.Domain.Models.Product;

namespace Pondrop.Service.Product.Application.Queries;

public class GetFullProductByIdQueryHandler : IRequestHandler<GetFullProductByIdQuery, Result<ProductViewRecord?>>
{
    private readonly IContainerRepository<ProductViewRecord> _viewRepository;
    private readonly IValidator<GetFullProductByIdQuery> _validator;
    private readonly ILogger<GetFullProductByIdQueryHandler> _logger;

    public GetFullProductByIdQueryHandler(
        IContainerRepository<ProductViewRecord> viewRepository,
        IValidator<GetFullProductByIdQuery> validator,
        ILogger<GetFullProductByIdQueryHandler> logger)
    {
        _viewRepository = viewRepository;
        _validator = validator;
        _logger = logger;
    }

    public async Task<Result<ProductViewRecord?>> Handle(GetFullProductByIdQuery query, CancellationToken cancellationToken)
    {
        var validation = _validator.Validate(query);

        if (!validation.IsValid)
        {
            var errorMessage = $"Get Product by id failed {validation}";
            _logger.LogError(errorMessage);
            return Result<ProductViewRecord?>.Error(errorMessage);
        }

        var result = default(Result<ProductViewRecord?>);

        try
        {
            var record = await _viewRepository.GetByIdAsync(query.Id);

            result = record is not null
                ? Result<ProductViewRecord?>.Success(record)
                : Result<ProductViewRecord?>.Success(null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            result = Result<ProductViewRecord?>.Error(ex);
        }

        return result;
    }
}