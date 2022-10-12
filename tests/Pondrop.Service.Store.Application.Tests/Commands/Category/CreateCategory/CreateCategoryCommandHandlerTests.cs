using AutoMapper;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Pondrop.Service.Product.Application.Commands;
using Pondrop.Service.Product.Application.Interfaces;
using Pondrop.Service.Product.Application.Interfaces.Services;
using Pondrop.Service.Product.Application.Models;
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

public class CreateCategoryCommandHandlerTests
{
    private readonly Mock<IOptions<CategoryUpdateConfiguration>> _CategoryUpdateConfigMock;
    private readonly Mock<IEventRepository> _eventRepositoryMock;
    private readonly Mock<ICheckpointRepository<CategoryEntity>> _checkpointRepositoryMock;
    private readonly Mock<IDaprService> _daprServiceMock;
    private readonly Mock<IUserService> _userServiceMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<IValidator<CreateCategoryCommand>> _validatorMock;
    private readonly Mock<ILogger<CreateCategoryCommandHandler>> _loggerMock;
    
    public CreateCategoryCommandHandlerTests()
    {
        _CategoryUpdateConfigMock = new Mock<IOptions<CategoryUpdateConfiguration>>();
        _eventRepositoryMock = new Mock<IEventRepository>();
        _checkpointRepositoryMock = new Mock<ICheckpointRepository<CategoryEntity>>();
        _daprServiceMock = new Mock<IDaprService>();
        _userServiceMock = new Mock<IUserService>();
        _mapperMock = new Mock<IMapper>();
        _validatorMock = new Mock<IValidator<CreateCategoryCommand>>();
        _loggerMock = new Mock<ILogger<CreateCategoryCommandHandler>>();

        _CategoryUpdateConfigMock
            .Setup(x => x.Value)
            .Returns(new CategoryUpdateConfiguration());
        _userServiceMock
            .Setup(x => x.CurrentUserId())
            .Returns("test/user");
    }
    
    [Fact]
    public async void CreateCategoryCommand_ShouldSucceed()
    {
        // arrange
        var cmd = CategoryFaker.GetCreateCategoryCommand();
        var item = CategoryFaker.GetCategoryRecord(cmd);
        _validatorMock
            .Setup(x => x.Validate(cmd))
            .Returns(new ValidationResult());
        _eventRepositoryMock
            .Setup(x => x.AppendEventsAsync(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<IEnumerable<IEvent>>()))
            .Returns(Task.FromResult(true));
        _mapperMock
            .Setup(x => x.Map<CategoryRecord>(It.IsAny<CategoryEntity>()))
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
            x => x.Map<CategoryRecord>(It.IsAny<CategoryEntity>()),
            Times.Once);
    }
    
    [Fact]
    public async void CreateCategoryCommand_WhenInvalid_ShouldFail()
    {
        // arrange
        var cmd = CategoryFaker.GetCreateCategoryCommand();
        var item = CategoryFaker.GetCategoryRecord(cmd);
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
            x => x.Map<CategoryRecord>(It.IsAny<CategoryEntity>()),
            Times.Never);
    }

    [Fact]
    public async void CreateCategoryCommand_WhenAppendEventsFail_ShouldFail()
    {
        // arrange
        var cmd = CategoryFaker.GetCreateCategoryCommand();
        var item = CategoryFaker.GetCategoryRecord(cmd);
        _validatorMock
            .Setup(x => x.Validate(cmd))
            .Returns(new ValidationResult());
        _eventRepositoryMock
            .Setup(x => x.AppendEventsAsync(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<IEnumerable<IEvent>>()))
            .Returns(Task.FromResult(false));
        _mapperMock
            .Setup(x => x.Map<CategoryRecord>(It.IsAny<CategoryEntity>()))
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
            x => x.Map<CategoryRecord>(It.IsAny<CategoryEntity>()),
            Times.Never);
    }
    
    [Fact]
    public async void CreateCategoryCommand_WhenException_ShouldFail()
    {
        // arrange
        var cmd = CategoryFaker.GetCreateCategoryCommand();
        var item = CategoryFaker.GetCategoryRecord(cmd);
        _validatorMock
            .Setup(x => x.Validate(cmd))
            .Returns(new ValidationResult());
        _eventRepositoryMock
            .Setup(x => x.AppendEventsAsync(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<IEnumerable<IEvent>>()))
            .Throws(new Exception());
        _mapperMock
            .Setup(x => x.Map<CategoryRecord>(It.IsAny<CategoryEntity>()))
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
            x => x.Map<CategoryRecord>(It.IsAny<CategoryEntity>()),
            Times.Never);
    }
    private CreateCategoryCommandHandler GetCommandHandler() =>
        new CreateCategoryCommandHandler(
            _CategoryUpdateConfigMock.Object,
            _checkpointRepositoryMock.Object,
            _eventRepositoryMock.Object,
            _daprServiceMock.Object,
            _userServiceMock.Object,
            _mapperMock.Object,
            _validatorMock.Object,
            _loggerMock.Object);
}