using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using Pondrop.Service.Interfaces;
using Pondrop.Service.Product.Application.Models;
using Pondrop.Service.Product.Domain.Models;

namespace Pondrop.Service.Product.Application.Queries;

public class GetAllBrandsQueryHandler : IRequestHandler<GetAllBrandsQuery, Result<List<BrandEntity>>>
{
    private readonly ICheckpointRepository<BrandEntity> _brandRepository;
    private readonly IMapper _mapper;
    private readonly IValidator<GetAllBrandsQuery> _validator;
    private readonly ILogger<GetAllBrandsQueryHandler> _logger;

    public GetAllBrandsQueryHandler(
        ICheckpointRepository<BrandEntity> brandRepository,
        IMapper mapper,
        IValidator<GetAllBrandsQuery> validator,
        ILogger<GetAllBrandsQueryHandler> logger)
    {
        _brandRepository = brandRepository;
        _mapper = mapper;
        _validator = validator;
        _logger = logger;
    }

    public async Task<Result<List<BrandEntity>>> Handle(GetAllBrandsQuery request, CancellationToken cancellationToken)
    {
        var validation = _validator.Validate(request);

        if (!validation.IsValid)
        {
            var errorMessage = $"Get all brands failed {validation}";
            _logger.LogError(errorMessage);
            return Result<List<BrandEntity>>.Error(errorMessage);
        }

        var result = default(Result<List<BrandEntity>>);

        try
        {
            var query = $"SELECT * FROM c";

            if (request.Offset != -1 && request.Limit != -1)
            {
                query += $" OFFSET {request.Offset} LIMIT {request.Limit}";
            }


            var records = await _brandRepository.QueryAsync(query);
            result = Result<List<BrandEntity>>.Success(records);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            result = Result<List<BrandEntity>>.Error(ex);
        }

        return result;
    }
}