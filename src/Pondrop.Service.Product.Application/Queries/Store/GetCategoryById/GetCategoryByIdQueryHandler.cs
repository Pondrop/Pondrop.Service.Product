using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using Pondrop.Service.Product.Application.Interfaces;
using Pondrop.Service.Product.Application.Models;
using Pondrop.Service.Product.Domain.Models;

namespace Pondrop.Service.Product.Application.Queries;

public class GetCategoryByIdQueryHandler : IRequestHandler<GetCategoryByIdQuery, Result<CategoryViewRecord?>>
{
    private readonly IContainerRepository<CategoryViewRecord> _viewRepository;
    private readonly IValidator<GetCategoryByIdQuery> _validator;
    private readonly ILogger<GetCategoryByIdQueryHandler> _logger;

    public GetCategoryByIdQueryHandler(
        IContainerRepository<CategoryViewRecord> viewRepository,
        IValidator<GetCategoryByIdQuery> validator,
        ILogger<GetCategoryByIdQueryHandler> logger)
    {
        _viewRepository = viewRepository;
        _validator = validator;
        _logger = logger;
    }

    public async Task<Result<CategoryViewRecord?>> Handle(GetCategoryByIdQuery query, CancellationToken cancellationToken)
    {
        var validation = _validator.Validate(query);

        if (!validation.IsValid)
        {
            var errorMessage = $"Get category by id failed {validation}";
            _logger.LogError(errorMessage);
            return Result<CategoryViewRecord?>.Error(errorMessage);
        }

        var result = default(Result<CategoryViewRecord?>);

        try
        {
            var record = await _viewRepository.GetByIdAsync(query.Id);

            result = record is not null
                ? Result<CategoryViewRecord?>.Success(record)
                : Result<CategoryViewRecord?>.Success(null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            result = Result<CategoryViewRecord?>.Error(ex);
        }

        return result;
    }
}