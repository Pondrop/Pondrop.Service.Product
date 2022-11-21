using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using Pondrop.Service.Interfaces;
using Pondrop.Service.Product.Application.Models;
using Pondrop.Service.Product.Domain.Models;

namespace Pondrop.Service.Product.Application.Queries;

public class GetAllCategoriesQueryHandler : IRequestHandler<GetAllCategoriesQuery, Result<List<CategoryEntity>>>
{
    private readonly ICheckpointRepository<CategoryEntity> _categoryRepository;
    private readonly IMapper _mapper;
    private readonly IValidator<GetAllCategoriesQuery> _validator;
    private readonly ILogger<GetAllCategoriesQueryHandler> _logger;

    public GetAllCategoriesQueryHandler(
        ICheckpointRepository<CategoryEntity> categoryRepository,
        IMapper mapper,
        IValidator<GetAllCategoriesQuery> validator,
        ILogger<GetAllCategoriesQueryHandler> logger)
    {
        _categoryRepository = categoryRepository;
        _mapper = mapper;
        _validator = validator;
        _logger = logger;
    }

    public async Task<Result<List<CategoryEntity>>> Handle(GetAllCategoriesQuery request, CancellationToken cancellationToken)
    {
        var validation = _validator.Validate(request);

        if (!validation.IsValid)
        {
            var errorMessage = $"Get all categorys failed {validation}";
            _logger.LogError(errorMessage);
            return Result<List<CategoryEntity>>.Error(errorMessage);
        }

        var result = default(Result<List<CategoryEntity>>);

        try
        {
            var query = $"SELECT * FROM c";

            if (request.Offset != -1 && request.Limit != -1)
            {
                query += $" OFFSET {request.Offset} LIMIT {request.Limit}";
            }


            var records = await _categoryRepository.QueryAsync(query);
            result = Result<List<CategoryEntity>>.Success(records);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            result = Result<List<CategoryEntity>>.Error(ex);
        }

        return result;
    }
}