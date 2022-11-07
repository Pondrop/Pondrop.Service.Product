using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using Pondrop.Service.Product.Application.Interfaces;
using Pondrop.Service.Product.Application.Models;
using Pondrop.Service.Product.Domain.Models;
using Pondrop.Service.Product.Domain.Models.Product;

namespace Pondrop.Service.Product.Application.Queries;

public class GetAllProductsQueryHandler : IRequestHandler<GetAllProductsQuery, Result<List<ProductEntity>>>
{
    private readonly ICheckpointRepository<ProductEntity> _ProductRepository;
    private readonly IMapper _mapper;
    private readonly IValidator<GetAllProductsQuery> _validator;
    private readonly ILogger<GetAllProductsQueryHandler> _logger;

    public GetAllProductsQueryHandler(
        ICheckpointRepository<ProductEntity> ProductRepository,
        IMapper mapper,
        IValidator<GetAllProductsQuery> validator,
        ILogger<GetAllProductsQueryHandler> logger)
    {
        _ProductRepository = ProductRepository;
        _mapper = mapper;
        _validator = validator;
        _logger = logger;
    }

    public async Task<Result<List<ProductEntity>>> Handle(GetAllProductsQuery request, CancellationToken cancellationToken)
    {
        var validation = _validator.Validate(request);

        if (!validation.IsValid)
        {
            var errorMessage = $"Get all Products failed {validation}";
            _logger.LogError(errorMessage);
            return Result<List<ProductEntity>>.Error(errorMessage);
        }

        var result = default(Result<List<ProductEntity>>);

        try
        {
            var records = await _ProductRepository.QueryAsync($"SELECT * FROM c OFFSET {request.Offset} LIMIT {request.Limit}");
            result = Result<List<ProductEntity>>.Success(records);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            result = Result<List<ProductEntity>>.Error(ex);
        }

        return result;
    }
}