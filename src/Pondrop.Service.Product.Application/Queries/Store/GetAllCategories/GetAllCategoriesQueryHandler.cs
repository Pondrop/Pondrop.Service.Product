using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using Pondrop.Service.Product.Application.Interfaces;
using Pondrop.Service.Product.Application.Models;
using Pondrop.Service.Product.Domain.Models;

namespace Pondrop.Service.Product.Application.Queries;

public class GetAllCategoriesQueryHandler : IRequestHandler<GetAllCategoriesQuery, Result<List<CategoryViewRecord>>>
{
    private readonly IContainerRepository<CategoryViewRecord> _containerRepository;
    private readonly IMapper _mapper;
    private readonly IValidator<GetAllCategoriesQuery> _validator;
    private readonly ILogger<GetAllCategoriesQueryHandler> _logger;

    public GetAllCategoriesQueryHandler(
        IContainerRepository<CategoryViewRecord> storeRepository,
        IMapper mapper,
        IValidator<GetAllCategoriesQuery> validator,
        ILogger<GetAllCategoriesQueryHandler> logger)
    {
        _containerRepository = storeRepository;
        _mapper = mapper;
        _validator = validator;
        _logger = logger;
    }

    public async Task<Result<List<CategoryViewRecord>>> Handle(GetAllCategoriesQuery request, CancellationToken cancellationToken)
    {
        var validation = _validator.Validate(request);

        if (!validation.IsValid)
        {
            var errorMessage = $"Get all stores failed {validation}";
            _logger.LogError(errorMessage);
            return Result<List<CategoryViewRecord>>.Error(errorMessage);
        }

        var result = default(Result<List<CategoryViewRecord>>);

        try
        {
            var records = await _containerRepository.GetAllAsync();
            result = Result<List<CategoryViewRecord>>.Success(records);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            result = Result<List<CategoryViewRecord>>.Error(ex);
        }

        return result;
    }
}