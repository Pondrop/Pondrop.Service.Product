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

namespace Pondrop.Service.Product.Application.Tests.Commands.Product.CreateCategoryGrouping;

public class GetCategoryGroupingByIdHandlerTests
{
    private readonly Mock<IContainerRepository<CategoryGroupingViewRecord>> _CategoryGroupingContainerRepositoryMock;
    private readonly Mock<IValidator<GetCategoryGroupingByIdQuery>> _validatorMock;
    private readonly Mock<ILogger<GetCategoryGroupingByIdQueryHandler>> _loggerMock;
    
    public GetCategoryGroupingByIdHandlerTests()
    {
        _CategoryGroupingContainerRepositoryMock = new Mock<IContainerRepository<CategoryGroupingViewRecord>>();
        _validatorMock = new Mock<IValidator<GetCategoryGroupingByIdQuery>>();
        _loggerMock = new Mock<ILogger<GetCategoryGroupingByIdQueryHandler>>();
    }
    
    [Fact]
    public async void GetCategoryGroupingByIdQuery_ShouldSucceed()
    {
        // arrange
        var query = new GetCategoryGroupingByIdQuery() { Id = Guid.NewGuid() };
        _validatorMock
            .Setup(x => x.Validate(query))
            .Returns(new ValidationResult());
        _CategoryGroupingContainerRepositoryMock
            .Setup(x => x.GetByIdAsync(query.Id))
            .Returns(Task.FromResult<CategoryGroupingViewRecord?>(new CategoryGroupingViewRecord()));
        var handler = GetQueryHandler();
        
        // act
        var result = await handler.Handle(query, CancellationToken.None);
        
        // assert
        Assert.True(result.IsSuccess);
        _validatorMock.Verify(
            x => x.Validate(query),
            Times.Once());
        _CategoryGroupingContainerRepositoryMock.Verify(
            x => x.GetByIdAsync(query.Id),
            Times.Once());
    }
    
    [Fact]
    public async void GetCategoryGroupingByIdQuery_WhenInvalid_ShouldFail()
    {
        // arrange
        var query = new GetCategoryGroupingByIdQuery() { Id = Guid.NewGuid() };
        _validatorMock
            .Setup(x => x.Validate(query))
            .Returns(new ValidationResult(new [] { new ValidationFailure() }));
        _CategoryGroupingContainerRepositoryMock
            .Setup(x => x.GetByIdAsync(query.Id))
            .Returns(Task.FromResult<CategoryGroupingViewRecord?>(new CategoryGroupingViewRecord()));
        var handler = GetQueryHandler();
        
        // act
        var result = await handler.Handle(query, CancellationToken.None);
        
        // assert
        Assert.False(result.IsSuccess);
        _validatorMock.Verify(
            x => x.Validate(query),
            Times.Once());
        _CategoryGroupingContainerRepositoryMock.Verify(
            x => x.GetByIdAsync(query.Id),
            Times.Never());
    }
    
    [Fact]
    public async void GetCategoryGroupingByIdQuery_WhenNotFound_ShouldSucceedWithNull()
    {
        // arrange
        var query = new GetCategoryGroupingByIdQuery() { Id = Guid.NewGuid() };
        _validatorMock
            .Setup(x => x.Validate(query))
            .Returns(new ValidationResult());
        _CategoryGroupingContainerRepositoryMock
            .Setup(x => x.GetByIdAsync(query.Id))
            .Returns(Task.FromResult<CategoryGroupingViewRecord?>(null));
        var handler = GetQueryHandler();
        
        // act
        var result = await handler.Handle(query, CancellationToken.None);
        
        // assert
        Assert.True(result.IsSuccess);
        Assert.Null(result.Value);
        _validatorMock.Verify(
            x => x.Validate(query),
            Times.Once());
        _CategoryGroupingContainerRepositoryMock.Verify(
            x => x.GetByIdAsync(query.Id),
            Times.Once());
    }
    
    [Fact]
    public async void GetCategoryGroupingByIdQuery_WhenThrows_ShouldFail()
    {
        // arrange
        var query = new GetCategoryGroupingByIdQuery() { Id = Guid.NewGuid() };
        var item = CategoryGroupingFaker.GetCategoryGroupingRecords(1).Single();
        _validatorMock
            .Setup(x => x.Validate(query))
            .Returns(new ValidationResult());
        _CategoryGroupingContainerRepositoryMock
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
        _CategoryGroupingContainerRepositoryMock.Verify(
            x => x.GetByIdAsync(query.Id),
            Times.Once());
    }
    
    private GetCategoryGroupingByIdQueryHandler GetQueryHandler() =>
        new GetCategoryGroupingByIdQueryHandler(
            _CategoryGroupingContainerRepositoryMock.Object,
            _validatorMock.Object,
            _loggerMock.Object);
}