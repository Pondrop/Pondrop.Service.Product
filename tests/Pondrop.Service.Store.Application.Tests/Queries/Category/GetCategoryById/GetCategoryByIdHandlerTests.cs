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

namespace Pondrop.Service.Product.Application.Tests.Commands.Product.CreateCategory;

public class GetCategoryByIdHandlerTests
{
    private readonly Mock<IContainerRepository<CategoryViewRecord>> _CategoryContainerRepositoryMock;
    private readonly Mock<IValidator<GetCategoryByIdQuery>> _validatorMock;
    private readonly Mock<ILogger<GetCategoryByIdQueryHandler>> _loggerMock;
    
    public GetCategoryByIdHandlerTests()
    {
        _CategoryContainerRepositoryMock = new Mock<IContainerRepository<CategoryViewRecord>>();
        _validatorMock = new Mock<IValidator<GetCategoryByIdQuery>>();
        _loggerMock = new Mock<ILogger<GetCategoryByIdQueryHandler>>();
    }
    
    [Fact]
    public async void GetCategoryByIdQuery_ShouldSucceed()
    {
        // arrange
        var query = new GetCategoryByIdQuery() { Id = Guid.NewGuid() };
        _validatorMock
            .Setup(x => x.Validate(query))
            .Returns(new ValidationResult());
        _CategoryContainerRepositoryMock
            .Setup(x => x.GetByIdAsync(query.Id))
            .Returns(Task.FromResult<CategoryViewRecord?>(new CategoryViewRecord()));
        var handler = GetQueryHandler();
        
        // act
        var result = await handler.Handle(query, CancellationToken.None);
        
        // assert
        Assert.True(result.IsSuccess);
        _validatorMock.Verify(
            x => x.Validate(query),
            Times.Once());
        _CategoryContainerRepositoryMock.Verify(
            x => x.GetByIdAsync(query.Id),
            Times.Once());
    }
    
    [Fact]
    public async void GetCategoryByIdQuery_WhenInvalid_ShouldFail()
    {
        // arrange
        var query = new GetCategoryByIdQuery() { Id = Guid.NewGuid() };
        _validatorMock
            .Setup(x => x.Validate(query))
            .Returns(new ValidationResult(new [] { new ValidationFailure() }));
        _CategoryContainerRepositoryMock
            .Setup(x => x.GetByIdAsync(query.Id))
            .Returns(Task.FromResult<CategoryViewRecord?>(new CategoryViewRecord()));
        var handler = GetQueryHandler();
        
        // act
        var result = await handler.Handle(query, CancellationToken.None);
        
        // assert
        Assert.False(result.IsSuccess);
        _validatorMock.Verify(
            x => x.Validate(query),
            Times.Once());
        _CategoryContainerRepositoryMock.Verify(
            x => x.GetByIdAsync(query.Id),
            Times.Never());
    }
    
    [Fact]
    public async void GetCategoryByIdQuery_WhenNotFound_ShouldSucceedWithNull()
    {
        // arrange
        var query = new GetCategoryByIdQuery() { Id = Guid.NewGuid() };
        _validatorMock
            .Setup(x => x.Validate(query))
            .Returns(new ValidationResult());
        _CategoryContainerRepositoryMock
            .Setup(x => x.GetByIdAsync(query.Id))
            .Returns(Task.FromResult<CategoryViewRecord?>(null));
        var handler = GetQueryHandler();
        
        // act
        var result = await handler.Handle(query, CancellationToken.None);
        
        // assert
        Assert.True(result.IsSuccess);
        Assert.Null(result.Value);
        _validatorMock.Verify(
            x => x.Validate(query),
            Times.Once());
        _CategoryContainerRepositoryMock.Verify(
            x => x.GetByIdAsync(query.Id),
            Times.Once());
    }
    
    [Fact]
    public async void GetCategoryByIdQuery_WhenThrows_ShouldFail()
    {
        // arrange
        var query = new GetCategoryByIdQuery() { Id = Guid.NewGuid() };
        var item = CategoryFaker.GetCategoryRecords(1).Single();
        _validatorMock
            .Setup(x => x.Validate(query))
            .Returns(new ValidationResult());
        _CategoryContainerRepositoryMock
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
        _CategoryContainerRepositoryMock.Verify(
            x => x.GetByIdAsync(query.Id),
            Times.Once());
    }
    
    private GetCategoryByIdQueryHandler GetQueryHandler() =>
        new GetCategoryByIdQueryHandler(
            _CategoryContainerRepositoryMock.Object,
            _validatorMock.Object,
            _loggerMock.Object);
}