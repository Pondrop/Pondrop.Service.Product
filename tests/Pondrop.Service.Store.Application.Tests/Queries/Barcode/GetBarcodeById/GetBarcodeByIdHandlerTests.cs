using AutoMapper;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;
using Moq;
using Pondrop.Service.Product.Application.Commands;
using Pondrop.Service.Product.Application.Interfaces;
using Pondrop.Service.Product.Application.Queries;
using Pondrop.Service.Product.Domain.Events;
using Pondrop.Service.Product.Domain.Models;
using Pondrop.Service.Product.Tests.Faker;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Pondrop.Service.Product.Application.Tests.Commands.Product.CreateBarcode;

public class GetBarcodeByIdHandlerTests
{
    private readonly Mock<ICheckpointRepository<BarcodeEntity>> _BarcodeContainerRepositoryMock;
    private readonly Mock<IValidator<GetBarcodeByIdQuery>> _validatorMock;
    private readonly Mock<ILogger<GetBarcodeByIdQueryHandler>> _loggerMock;
    
    public GetBarcodeByIdHandlerTests()
    {
        _BarcodeContainerRepositoryMock = new Mock<ICheckpointRepository<BarcodeEntity>>();
        _validatorMock = new Mock<IValidator<GetBarcodeByIdQuery>>();
        _loggerMock = new Mock<ILogger<GetBarcodeByIdQueryHandler>>();
    }
    
    [Fact]
    public async void GetBarcodeByIdQuery_ShouldSucceed()
    {
        // arrange
        var query = new GetBarcodeByIdQuery() { Id = Guid.NewGuid() };
        _validatorMock
            .Setup(x => x.Validate(query))
            .Returns(new ValidationResult());
        _BarcodeContainerRepositoryMock
            .Setup(x => x.GetByIdAsync(query.Id))
            .Returns(Task.FromResult<BarcodeEntity?>(new BarcodeEntity()));
        var handler = GetQueryHandler();
        
        // act
        var result = await handler.Handle(query, CancellationToken.None);
        
        // assert
        Assert.True(result.IsSuccess);
        _validatorMock.Verify(
            x => x.Validate(query),
            Times.Once());
        _BarcodeContainerRepositoryMock.Verify(
            x => x.GetByIdAsync(query.Id),
            Times.Once());
    }
    
    [Fact]
    public async void GetBarcodeByIdQuery_WhenInvalid_ShouldFail()
    {
        // arrange
        var query = new GetBarcodeByIdQuery() { Id = Guid.NewGuid() };
        _validatorMock
            .Setup(x => x.Validate(query))
            .Returns(new ValidationResult(new [] { new ValidationFailure() }));
        _BarcodeContainerRepositoryMock
            .Setup(x => x.GetByIdAsync(query.Id))
            .Returns(Task.FromResult<BarcodeEntity?>(new BarcodeEntity()));
        var handler = GetQueryHandler();
        
        // act
        var result = await handler.Handle(query, CancellationToken.None);
        
        // assert
        Assert.False(result.IsSuccess);
        _validatorMock.Verify(
            x => x.Validate(query),
            Times.Once());
        _BarcodeContainerRepositoryMock.Verify(
            x => x.GetByIdAsync(query.Id),
            Times.Never());
    }
    
    [Fact]
    public async void GetBarcodeByIdQuery_WhenNotFound_ShouldSucceedWithNull()
    {
        // arrange
        var query = new GetBarcodeByIdQuery() { Id = Guid.NewGuid() };
        _validatorMock
            .Setup(x => x.Validate(query))
            .Returns(new ValidationResult());
        _BarcodeContainerRepositoryMock
            .Setup(x => x.GetByIdAsync(query.Id))
            .Returns(Task.FromResult<BarcodeEntity?>(null));
        var handler = GetQueryHandler();
        
        // act
        var result = await handler.Handle(query, CancellationToken.None);
        
        // assert
        Assert.True(result.IsSuccess);
        Assert.Null(result.Value);
        _validatorMock.Verify(
            x => x.Validate(query),
            Times.Once());
        _BarcodeContainerRepositoryMock.Verify(
            x => x.GetByIdAsync(query.Id),
            Times.Once());
    }
    
    [Fact]
    public async void GetBarcodeByIdQuery_WhenThrows_ShouldFail()
    {
        // arrange
        var query = new GetBarcodeByIdQuery() { Id = Guid.NewGuid() };
        var item = BarcodeFaker.GetBarcodeRecords(1).Single();
        _validatorMock
            .Setup(x => x.Validate(query))
            .Returns(new ValidationResult());
        _BarcodeContainerRepositoryMock
            .Setup(x => x.GetByIdAsync(query.Id))
            .Throws(new Exception());
        var handler = GetQueryHandler();
        
        // act
        var result = await handler.Handle(query, CancellationToken.None);
        
        // assert
        Assert.False(result.IsSuccess);
        _validatorMock.Verify(
            x => x.Validate(query),
            Times.Once());
        _BarcodeContainerRepositoryMock.Verify(
            x => x.GetByIdAsync(query.Id),
            Times.Once());
    }
    
    private GetBarcodeByIdQueryHandler GetQueryHandler() =>
        new GetBarcodeByIdQueryHandler(
            _BarcodeContainerRepositoryMock.Object,
            _validatorMock.Object,
            _loggerMock.Object);
}