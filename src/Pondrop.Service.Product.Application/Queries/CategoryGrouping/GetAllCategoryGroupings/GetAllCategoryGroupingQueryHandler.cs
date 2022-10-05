using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using Pondrop.Service.Product.Application.Interfaces;
using Pondrop.Service.Product.Application.Models;
using Pondrop.Service.Product.Domain.Models;

namespace Pondrop.Service.Product.Application.Queries;

public class GetAllCategoryGroupingsQueryHandler : IRequestHandler<GetAllCategoryGroupingsQuery, Result<List<CategoryGroupingEntity>>>
{
    private readonly ICheckpointRepository<CategoryGroupingEntity> _categoryGroupingRepository;
    private readonly IMapper _mapper;
    private readonly IValidator<GetAllCategoryGroupingsQuery> _validator;
    private readonly ILogger<GetAllCategoryGroupingsQueryHandler> _logger;

    public GetAllCategoryGroupingsQueryHandler(
        ICheckpointRepository<CategoryGroupingEntity> categoryGroupingRepository,
        IMapper mapper,
        IValidator<GetAllCategoryGroupingsQuery> validator,
        ILogger<GetAllCategoryGroupingsQueryHandler> logger)
    {
        _categoryGroupingRepository = categoryGroupingRepository;
        _mapper = mapper;
        _validator = validator;
        _logger = logger;
    }

    public async Task<Result<List<CategoryGroupingEntity>>> Handle(GetAllCategoryGroupingsQuery request, CancellationToken cancellationToken)
    {
        var validation = _validator.Validate(request);

        if (!validation.IsValid)
        {
            var errorMessage = $"Get all categorys failed {validation}";
            _logger.LogError(errorMessage);
            return Result<List<CategoryGroupingEntity>>.Error(errorMessage);
        }

        var result = default(Result<List<CategoryGroupingEntity>>);

        try
        {
            var records = await _categoryGroupingRepository.GetAllAsync();
            result = Result<List<CategoryGroupingEntity>>.Success(records);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            result = Result<List<CategoryGroupingEntity>>.Error(ex);
        }

        return result;
    }
}