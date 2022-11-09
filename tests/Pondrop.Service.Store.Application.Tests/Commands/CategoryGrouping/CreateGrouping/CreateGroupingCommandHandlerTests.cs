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
using Pondrop.Service.Product.Domain.Models;
using Pondrop.Service.Product.Tests.Faker;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Pondrop.Service.Product.Application.Tests.Commands.Product.CreateCategoryGrouping;

public class CreateCategoryGroupingCommandHandlerTests
{
    private readonly Mock<IOptions<CategoryUpdateConfiguration>> _CategoryGroupingUpdateConfigMock;
    private readonly Mock<IEventRepository> _eventRepositoryMock;
    private readonly Mock<IDaprService> _daprServiceMock;
    private readonly Mock<IUserService> _userServiceMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<IValidator<CreateCategoryGroupingCommand>> _validatorMock;
    private readonly Mock<ILogger<CreateCategoryGroupingCommandHandler>> _loggerMock;
    
    public CreateCategoryGroupingCommandHandlerTests()
    {
        _CategoryGroupingUpdateConfigMock = new Mock<IOptions<CategoryUpdateConfiguration>>();
        _eventRepositoryMock = new Mock<IEventRepository>();
        _daprServiceMock = new Mock<IDaprService>();
        _userServiceMock = new Mock<IUserService>();
        _mapperMock = new Mock<IMapper>();
        _validatorMock = new Mock<IValidator<CreateCategoryGroupingCommand>>();
        _loggerMock = new Mock<ILogger<CreateCategoryGroupingCommandHandler>>();

        _CategoryGroupingUpdateConfigMock
            .Setup(x => x.Value)
            .Returns(new CategoryUpdateConfiguration());
        _userServiceMock
            .Setup(x => x.CurrentUserId())
            .Returns("test/user");
    }
    
    [Fact]
    public async void CreateCategoryGroupingCommand_ShouldSucceed()
    {
        // arrange
        var cmd = CategoryGroupingFaker.GetCreateCategoryGroupingCommand();
        var item = CategoryGroupingFaker.GetCategoryGroupingRecord(cmd);
        _validatorMock
            .Setup(x => x.Validate(cmd))
            .Returns(new ValidationResult());
        _eventRepositoryMock
            .Setup(x => x.AppendEventsAsync(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<IEnumerable<IEvent>>()))
            .Returns(Task.FromResult(true));
        _mapperMock
            .Setup(x => x.Map<CategoryGroupingRecord>(It.IsAny<CategoryGroupingEntity>()))
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
            x => x.Map<CategoryGroupingRecord>(It.IsAny<CategoryGroupingEntity>()),
            Times.Once);
    }
    
    [Fact]
    public async void CreateCategoryGroupingCommand_WhenInvalid_ShouldFail()
    {
        // arrange
        var cmd = CategoryGroupingFaker.GetCreateCategoryGroupingCommand();
        var item = CategoryGroupingFaker.GetCategoryGroupingRecord(cmd);
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
            x => x.Map<CategoryGroupingRecord>(It.IsAny<CategoryGroupingEntity>()),
            Times.Never);
    }

    [Fact]
    public async void CreateCategoryGroupingCommand_WhenAppendEventsFail_ShouldFail()
    {
        // arrange
        var cmd = CategoryGroupingFaker.GetCreateCategoryGroupingCommand();
        var item = CategoryGroupingFaker.GetCategoryGroupingRecord(cmd);
        _validatorMock
            .Setup(x => x.Validate(cmd))
            .Returns(new ValidationResult());
        _eventRepositoryMock
            .Setup(x => x.AppendEventsAsync(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<IEnumerable<IEvent>>()))
            .Returns(Task.FromResult(false));
        _mapperMock
            .Setup(x => x.Map<CategoryGroupingRecord>(It.IsAny<CategoryGroupingEntity>()))
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
            x => x.Map<CategoryGroupingRecord>(It.IsAny<CategoryGroupingEntity>()),
            Times.Never);
    }
    
    [Fact]
    public async void CreateCategoryGroupingCommand_WhenException_ShouldFail()
    {
        // arrange
        var cmd = CategoryGroupingFaker.GetCreateCategoryGroupingCommand();
        var item = CategoryGroupingFaker.GetCategoryGroupingRecord(cmd);
        _validatorMock
            .Setup(x => x.Validate(cmd))
            .Returns(new ValidationResult());
        _eventRepositoryMock
            .Setup(x => x.AppendEventsAsync(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<IEnumerable<IEvent>>()))
            .Throws(new Exception());
        _mapperMock
            .Setup(x => x.Map<CategoryGroupingRecord>(It.IsAny<CategoryGroupingEntity>()))
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
            x => x.Map<CategoryGroupingRecord>(It.IsAny<CategoryGroupingEntity>()),
            Times.Never);
    }
    private CreateCategoryGroupingCommandHandler GetCommandHandler() =>
        new CreateCategoryGroupingCommandHandler(
            _CategoryGroupingUpdateConfigMock.Object,
            _eventRepositoryMock.Object,
            _daprServiceMock.Object,
            _userServiceMock.Object,
            _mapperMock.Object,
            _validatorMock.Object,
            _loggerMock.Object);
}