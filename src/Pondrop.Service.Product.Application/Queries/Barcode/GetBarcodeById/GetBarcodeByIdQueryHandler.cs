using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using Pondrop.Service.Interfaces;
using Pondrop.Service.Product.Application.Models;
using Pondrop.Service.Product.Domain.Models;

namespace Pondrop.Service.Product.Application.Queries;

public class GetBarcodeByIdQueryHandler : IRequestHandler<GetBarcodeByIdQuery, Result<BarcodeEntity?>>
{
    private readonly ICheckpointRepository<BarcodeEntity> _viewRepository;
    private readonly IValidator<GetBarcodeByIdQuery> _validator;
    private readonly ILogger<GetBarcodeByIdQueryHandler> _logger;

    public GetBarcodeByIdQueryHandler(
        ICheckpointRepository<BarcodeEntity> viewRepository,
        IValidator<GetBarcodeByIdQuery> validator,
        ILogger<GetBarcodeByIdQueryHandler> logger)
    {
        _viewRepository = viewRepository;
        _validator = validator;
        _logger = logger;
    }

    public async Task<Result<BarcodeEntity?>> Handle(GetBarcodeByIdQuery query, CancellationToken cancellationToken)
    {
        var validation = _validator.Validate(query);

        if (!validation.IsValid)
        {
            var errorMessage = $"Get barcode by id failed {validation}";
            _logger.LogError(errorMessage);
            return Result<BarcodeEntity?>.Error(errorMessage);
        }

        var result = default(Result<BarcodeEntity?>);

        try
        {
            var record = await _viewRepository.GetByIdAsync(query.Id);

            result = record is not null
                ? Result<BarcodeEntity?>.Success(record)
                : Result<BarcodeEntity?>.Success(null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            result = Result<BarcodeEntity?>.Error(ex);
        }

        return result;
    }
}