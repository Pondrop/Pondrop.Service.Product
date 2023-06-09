﻿using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using Pondrop.Service.Interfaces;
using Pondrop.Service.Product.Application.Models;
using Pondrop.Service.Product.Domain.Models;

namespace Pondrop.Service.Product.Application.Queries;

public class GetAllBarcodesQueryHandler : IRequestHandler<GetAllBarcodesQuery, Result<List<BarcodeEntity>>>
{
    private readonly ICheckpointRepository<BarcodeEntity> _barcodeRepository;
    private readonly IMapper _mapper;
    private readonly IValidator<GetAllBarcodesQuery> _validator;
    private readonly ILogger<GetAllBarcodesQueryHandler> _logger;

    public GetAllBarcodesQueryHandler(
        ICheckpointRepository<BarcodeEntity> barcodeRepository,
        IMapper mapper,
        IValidator<GetAllBarcodesQuery> validator,
        ILogger<GetAllBarcodesQueryHandler> logger)
    {
        _barcodeRepository = barcodeRepository;
        _mapper = mapper;
        _validator = validator;
        _logger = logger;
    }

    public async Task<Result<List<BarcodeEntity>>> Handle(GetAllBarcodesQuery request, CancellationToken cancellationToken)
    {
        var validation = _validator.Validate(request);

        if (!validation.IsValid)
        {
            var errorMessage = $"Get all barcodes failed {validation}";
            _logger.LogError(errorMessage);
            return Result<List<BarcodeEntity>>.Error(errorMessage);
        }

        var result = default(Result<List<BarcodeEntity>>);

        try
        {
            var query = $"SELECT * FROM c";

            if (request.Offset != -1 && request.Limit != -1)
            {
                query += $" OFFSET {request.Offset} LIMIT {request.Limit}";
            }


            var records = await _barcodeRepository.QueryAsync(query);
            result = Result<List<BarcodeEntity>>.Success(records);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            result = Result<List<BarcodeEntity>>.Error(ex);
        }

        return result;
    }
}