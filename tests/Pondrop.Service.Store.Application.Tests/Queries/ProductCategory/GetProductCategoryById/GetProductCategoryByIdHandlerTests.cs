using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;
using Moq;
using Pondrop.Service.Interfaces;
using Pondrop.Service.Product.Application.Queries;
using Pondrop.Service.Product.Tests.Faker;
using Pondrop.Service.ProductCategory.Domain.Models;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Pondrop.Service.Product.Application.Tests.Commands.Product.CreateProductCategory;

public class GetProductCategoryByIdHandlerTests
{
    private readonly Mock<ICheckpointRepository<ProductCategoryEntity>> _ProductCategoryContainerRepositoryMock;
    private readonly Mock<IValidator<GetProductCategoryByIdQuery>> _validatorMock;
    private readonly Mock<ILogger<GetProductCategoryByIdQueryHandler>> _loggerMock;
    
    public GetProductCategoryByIdHandlerTests()
    {
        _ProductCategoryContainerRepositoryMock = new Mock<ICheckpointRepository<ProductCategoryEntity>>();
        _validatorMock = new Mock<IValidator<GetProductCategoryByIdQuery>>();
        _loggerMock = new Mock<ILogger<GetProductCategoryByIdQueryHandler>>();
    }
    
    [Fact]
    public async void GetProductCategoryByIdQuery_ShouldSucceed()
    {
        // arrange
        var query = new GetProductCategoryByIdQuery() { Id = Guid.NewGuid() };
        _validatorMock
            .Setup(x => x.Validate(query))
            .Returns(new ValidationResult());
        _ProductCategoryContainerRepositoryMock
            .Setup(x => x.GetByIdAsync(query.Id))
            .Returns(Task.FromResult<ProductCategoryEntity?>(new ProductCategoryEntity()));
        var handler = GetQueryHandler();
        
        // act
        var result = await handler.Handle(query, CancellationToken.None);
        
        // assert
        Assert.True(result.IsSuccess);
        _validatorMock.Verify(
            x => x.Validate(query),
            Times.Once());
        _ProductCategoryContainerRepositoryMock.Verify(
            x => x.GetByIdAsync(query.Id),
            Times.Once());
    }
    
    [Fact]
    public async void GetProductCategoryByIdQuery_WhenInvalid_ShouldFail()
    {
        // arrange
        var query = new GetProductCategoryByIdQuery() { Id = Guid.NewGuid() };
        _validatorMock
            .Setup(x => x.Validate(query))
            .Returns(new ValidationResult(new [] { new ValidationFailure() }));
        _ProductCategoryContainerRepositoryMock
            .Setup(x => x.GetByIdAsync(query.Id))
            .Returns(Task.FromResult<ProductCategoryEntity?>(new ProductCategoryEntity()));
        var handler = GetQueryHandler();
        
        // act
        var result = await handler.Handle(query, CancellationToken.None);
        
        // assert
        Assert.False(result.IsSuccess);
        _validatorMock.Verify(
            x => x.Validate(query),
            Times.Once());
        _ProductCategoryContainerRepositoryMock.Verify(
            x => x.GetByIdAsync(query.Id),
            Times.Never());
    }
    
    [Fact]
    public async void GetProductCategoryByIdQuery_WhenNotFound_ShouldSucceedWithNull()
    {
        // arrange
        var query = new GetProductCategoryByIdQuery() { Id = Guid.NewGuid() };
        _validatorMock
            .Setup(x => x.Validate(query))
            .Returns(new ValidationResult());
        _ProductCategoryContainerRepositoryMock
            .Setup(x => x.GetByIdAsync(query.Id))
            .Returns(Task.FromResult<ProductCategoryEntity?>(null));
        var handler = GetQueryHandler();
        
        // act
        var result = await handler.Handle(query, CancellationToken.None);
        
        // assert
        Assert.True(result.IsSuccess);
        Assert.Null(result.Value);
        _validatorMock.Verify(
            x => x.Validate(query),
            Times.Once());
        _ProductCategoryContainerRepositoryMock.Verify(
            x => x.GetByIdAsync(query.Id),
            Times.Once());
    }
    
    [Fact]
    public async void GetProductCategoryByIdQuery_WhenThrows_ShouldFail()
    {
        // arrange
        var query = new GetProductCategoryByIdQuery() { Id = Guid.NewGuid() };
        var item = ProductCategoryFaker.GetProductCategoryRecords(1).Single();
        _validatorMock
            .Setup(x => x.Validate(query))
            .Returns(new ValidationResult());
        _ProductCategoryContainerRepositoryMock
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
        _ProductCategoryContainerRepositoryMock.Verify(
            x => x.GetByIdAsync(query.Id),
            Times.Once());
    }
    
    private GetProductCategoryByIdQueryHandler GetQueryHandler() =>
        new GetProductCategoryByIdQueryHandler(
            _ProductCategoryContainerRepositoryMock.Object,
            _validatorMock.Object,
            _loggerMock.Object);
}