using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using Pondrop.Service.Product.Application.Interfaces;
using Pondrop.Service.Product.Application.Models;
using Pondrop.Service.ProductCategory.Domain.Models;

namespace Pondrop.Service.Product.Application.Queries;

public class GetAllProductCategoriesQueryHandler : IRequestHandler<GetAllProductCategoriesQuery, Result<List<ProductCategoryEntity>>>
{
    private readonly ICheckpointRepository<ProductCategoryEntity> _ProductCategoryRepository;
    private readonly IMapper _mapper;
    private readonly IValidator<GetAllProductCategoriesQuery> _validator;
    private readonly ILogger<GetAllProductCategoriesQueryHandler> _logger;

    public GetAllProductCategoriesQueryHandler(
        ICheckpointRepository<ProductCategoryEntity> ProductCategoryRepository,
        IMapper mapper,
        IValidator<GetAllProductCategoriesQuery> validator,
        ILogger<GetAllProductCategoriesQueryHandler> logger)
    {
        _ProductCategoryRepository = ProductCategoryRepository;
        _mapper = mapper;
        _validator = validator;
        _logger = logger;
    }

    public async Task<Result<List<ProductCategoryEntity>>> Handle(GetAllProductCategoriesQuery request, CancellationToken cancellationToken)
    {
        var validation = _validator.Validate(request);

        if (!validation.IsValid)
        {
            var errorMessage = $"Get all ProductCategories failed {validation}";
            _logger.LogError(errorMessage);
            return Result<List<ProductCategoryEntity>>.Error(errorMessage);
        }

        var result = default(Result<List<ProductCategoryEntity>>);

        try
        {
            var records = await _ProductCategoryRepository.GetAllAsync();
            result = Result<List<ProductCategoryEntity>>.Success(records);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            result = Result<List<ProductCategoryEntity>>.Error(ex);
        }

        return result;
    }
}