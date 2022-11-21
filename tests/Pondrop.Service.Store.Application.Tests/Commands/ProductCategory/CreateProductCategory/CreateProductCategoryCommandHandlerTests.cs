using AutoMapper;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Pondrop.Service.Events;
using Pondrop.Service.Interfaces;
using Pondrop.Service.Interfaces.Services;
using Pondrop.Service.Product.Application.Commands;
using Pondrop.Service.Product.Application.Models;
using Pondrop.Service.Product.Tests.Faker;
using Pondrop.Service.ProductCategory.Domain.Models;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Pondrop.Service.Product.Application.Tests.Commands.Product.CreateProductCategory;

public class CreateProductCategoryCommandHandlerTests
{
    private readonly Mock<IOptions<ProductCategoryUpdateConfiguration>> _ProductCategoryUpdateConfigMock;
    private readonly Mock<IEventRepository> _eventRepositoryMock;
    private readonly Mock<IDaprService> _daprServiceMock;
    private readonly Mock<IUserService> _userServiceMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<IValidator<CreateProductCategoryCommand>> _validatorMock;
    private readonly Mock<ILogger<CreateProductCategoryCommandHandler>> _loggerMock;
    
    public CreateProductCategoryCommandHandlerTests()
    {
        _ProductCategoryUpdateConfigMock = new Mock<IOptions<ProductCategoryUpdateConfiguration>>();
        _eventRepositoryMock = new Mock<IEventRepository>();
        _daprServiceMock = new Mock<IDaprService>();
        _userServiceMock = new Mock<IUserService>();
        _mapperMock = new Mock<IMapper>();
        _validatorMock = new Mock<IValidator<CreateProductCategoryCommand>>();
        _loggerMock = new Mock<ILogger<CreateProductCategoryCommandHandler>>();

        _ProductCategoryUpdateConfigMock
            .Setup(x => x.Value)
            .Returns(new ProductCategoryUpdateConfiguration());
        _userServiceMock
            .Setup(x => x.CurrentUserId())
            .Returns("test/user");
    }
    
    [Fact]
    public async void CreateProductCategoryCommand_ShouldSucceed()
    {
        // arrange
        var cmd = ProductCategoryFaker.GetCreateProductCategoryCommand();
        var item = ProductCategoryFaker.GetProductCategoryRecord(cmd);
        _validatorMock
            .Setup(x => x.Validate(cmd))
            .Returns(new ValidationResult());
        _eventRepositoryMock
            .Setup(x => x.AppendEventsAsync(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<IEnumerable<IEvent>>()))
            .Returns(Task.FromResult(true));
        _mapperMock
            .Setup(x => x.Map<ProductCategoryRecord>(It.IsAny<ProductCategoryEntity>()))
            .Returns(item);
        var handler = GetCommandHandler();
        
        // act
        var result = await handler.Handle(cmd, CancellationToken.None);
        
        // assert
        Assert.True(result.IsSuccess);
        Assert.Equal(item, result.Value);
        _validatorMock.Verify(
            x => x.Validate(cmd),
            Times.Once());
        _eventRepositoryMock.Verify(
            x => x.AppendEventsAsync(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<IEnumerable<IEvent>>()),
            Times.Once());
        _mapperMock.Verify(
            x => x.Map<ProductCategoryRecord>(It.IsAny<ProductCategoryEntity>()),
            Times.Once);
    }
    
    [Fact]
    public async void CreateProductCategoryCommand_WhenInvalid_ShouldFail()
    {
        // arrange
        var cmd = ProductCategoryFaker.GetCreateProductCategoryCommand();
        var item = ProductCategoryFaker.GetProductCategoryRecord(cmd);
        _validatorMock
            .Setup(x => x.Validate(cmd))
            .Returns(new ValidationResult(new [] { new ValidationFailure() }));
        var handler = GetCommandHandler();
        
        // act
        var result = await handler.Handle(cmd, CancellationToken.None);
        
        // assert
        Assert.False(result.IsSuccess);
        _validatorMock.Verify(
            x => x.Validate(cmd),
            Times.Once());
        _eventRepositoryMock.Verify(
            x => x.AppendEventsAsync(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<IEnumerable<IEvent>>()),
            Times.Never());
        _mapperMock.Verify(
            x => x.Map<ProductCategoryRecord>(It.IsAny<ProductCategoryEntity>()),
            Times.Never);
    }

    [Fact]
    public async void CreateProductCategoryCommand_WhenAppendEventsFail_ShouldFail()
    {
        // arrange
        var cmd = ProductCategoryFaker.GetCreateProductCategoryCommand();
        var item = ProductCategoryFaker.GetProductCategoryRecord(cmd);
        _validatorMock
            .Setup(x => x.Validate(cmd))
            .Returns(new ValidationResult());
        _eventRepositoryMock
            .Setup(x => x.AppendEventsAsync(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<IEnumerable<IEvent>>()))
            .Returns(Task.FromResult(false));
        _mapperMock
            .Setup(x => x.Map<ProductCategoryRecord>(It.IsAny<ProductCategoryEntity>()))
            .Returns(item);
        var handler = GetCommandHandler();
        
        // act
        var result = await handler.Handle(cmd, CancellationToken.None);
        
        // assert
        Assert.False(result.IsSuccess);
        _validatorMock.Verify(
            x => x.Validate(cmd),
            Times.Once());
        _eventRepositoryMock.Verify(
            x => x.AppendEventsAsync(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<IEnumerable<IEvent>>()),
            Times.Once());
        _mapperMock.Verify(
            x => x.Map<ProductCategoryRecord>(It.IsAny<ProductCategoryEntity>()),
            Times.Never);
    }
    
    [Fact]
    public async void CreateProductCategoryCommand_WhenException_ShouldFail()
    {
        // arrange
        var cmd = ProductCategoryFaker.GetCreateProductCategoryCommand();
        var item = ProductCategoryFaker.GetProductCategoryRecord(cmd);
        _validatorMock
            .Setup(x => x.Validate(cmd))
            .Returns(new ValidationResult());
        _eventRepositoryMock
            .Setup(x => x.AppendEventsAsync(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<IEnumerable<IEvent>>()))
            .Throws(new Exception());
        _mapperMock
            .Setup(x => x.Map<ProductCategoryRecord>(It.IsAny<ProductCategoryEntity>()))
            .Returns(item);
        var handler = GetCommandHandler();
        
        // act
        var result = await handler.Handle(cmd, CancellationToken.None);
        
        // assert
        Assert.False(result.IsSuccess);
        _validatorMock.Verify(
            x => x.Validate(cmd),
            Times.Once());
        _eventRepositoryMock.Verify(
            x => x.AppendEventsAsync(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<IEnumerable<IEvent>>()),
            Times.Once());
        _mapperMock.Verify(
            x => x.Map<ProductCategoryRecord>(It.IsAny<ProductCategoryEntity>()),
            Times.Never);
    }
    private CreateProductCategoryCommandHandler GetCommandHandler() =>
        new CreateProductCategoryCommandHandler(
            _ProductCategoryUpdateConfigMock.Object,
            _eventRepositoryMock.Object,
            _daprServiceMock.Object,
            _userServiceMock.Object,
            _mapperMock.Object,
            _validatorMock.Object,
            _loggerMock.Object);
}