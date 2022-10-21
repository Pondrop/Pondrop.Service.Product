using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using Pondrop.Service.Product.Application.Interfaces;
using Pondrop.Service.Product.Application.Models;
using Pondrop.Service.Product.Domain.Models.ProductCategory;

namespace Pondrop.Service.Product.Application.Queries;

public class GetAllParentCategoriesQueryHandler : IRequestHandler<GetAllParentCategoriesQuery, Result<List<ParentCategoryViewRecord>>>
{
    private readonly IContainerRepository<ParentCategoryViewRecord> _parentCategoryContainerRepository;
    private readonly IMapper _mapper;
    private readonly IValidator<GetAllParentCategoriesQuery> _validator;
    private readonly ILogger<GetAllParentCategoriesQueryHandler> _logger;

    public GetAllParentCategoriesQueryHandler(
        IContainerRepository<ParentCategoryViewRecord> parentCategoryContainerRepository,
        IMapper mapper,
        IValidator<GetAllParentCategoriesQuery> validator,
        ILogger<GetAllParentCategoriesQueryHandler> logger)
    {
        _parentCategoryContainerRepository = parentCategoryContainerRepository;
        _mapper = mapper;
        _validator = validator;
        _logger = logger;
    }

    public async Task<Result<List<ParentCategoryViewRecord>>> Handle(GetAllParentCategoriesQuery request, CancellationToken cancellationToken)
    {
        var validation = _validator.Validate(request);

        if (!validation.IsValid)
        {
            var errorMessage = $"Get all ParentCategories failed {validation}";
            _logger.LogError(errorMessage);
            return Result<List<ParentCategoryViewRecord>>.Error(errorMessage);
        }

        var result = default(Result<List<ParentCategoryViewRecord>>);

        try
        {
            var records = await _parentCategoryContainerRepository.GetAllAsync();
            result = Result<List<ParentCategoryViewRecord>>.Success(records);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            result = Result<List<ParentCategoryViewRecord>>.Error(ex);
        }

        return result;
    }
}