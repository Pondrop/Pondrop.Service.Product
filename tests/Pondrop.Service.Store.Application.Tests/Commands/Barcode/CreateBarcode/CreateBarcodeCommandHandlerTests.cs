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

namespace Pondrop.Service.Product.Application.Tests.Commands.Product.CreateBarcode;

public class CreateBarcodeCommandHandlerTests
{
    private readonly Mock<IOptions<BarcodeUpdateConfiguration>> _BarcodeUpdateConfigMock;
    private readonly Mock<IEventRepository> _eventRepositoryMock;
    private readonly Mock<IDaprService> _daprServiceMock;
    private readonly Mock<IUserService> _userServiceMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<IValidator<CreateBarcodeCommand>> _validatorMock;
    private readonly Mock<ILogger<CreateBarcodeCommandHandler>> _loggerMock;
    
    public CreateBarcodeCommandHandlerTests()
    {
        _BarcodeUpdateConfigMock = new Mock<IOptions<BarcodeUpdateConfiguration>>();
        _eventRepositoryMock = new Mock<IEventRepository>();
        _daprServiceMock = new Mock<IDaprService>();
        _userServiceMock = new Mock<IUserService>();
        _mapperMock = new Mock<IMapper>();
        _validatorMock = new Mock<IValidator<CreateBarcodeCommand>>();
        _loggerMock = new Mock<ILogger<CreateBarcodeCommandHandler>>();

        _BarcodeUpdateConfigMock
            .Setup(x => x.Value)
            .Returns(new BarcodeUpdateConfiguration());
        _userServiceMock
            .Setup(x => x.CurrentUserId())
            .Returns("test/user");
    }
    
    [Fact]
    public async void CreateBarcodeCommand_ShouldSucceed()
    {
        // arrange
        var cmd = BarcodeFaker.GetCreateBarcodeCommand();
        var item = BarcodeFaker.GetBarcodeRecord(cmd);
        _validatorMock
            .Setup(x => x.Validate(cmd))
            .Returns(new ValidationResult());
        _eventRepositoryMock
            .Setup(x => x.AppendEventsAsync(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<IEnumerable<IEvent>>()))
            .Returns(Task.FromResult(true));
        _mapperMock
            .Setup(x => x.Map<BarcodeRecord>(It.IsAny<BarcodeEntity>()))
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
            x => x.Map<BarcodeRecord>(It.IsAny<BarcodeEntity>()),
            Times.Once);
    }
    
    [Fact]
    public async void CreateBarcodeCommand_WhenInvalid_ShouldFail()
    {
        // arrange
        var cmd = BarcodeFaker.GetCreateBarcodeCommand();
        var item = BarcodeFaker.GetBarcodeRecord(cmd);
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
            x => x.Map<BarcodeRecord>(It.IsAny<BarcodeEntity>()),
            Times.Never);
    }

    [Fact]
    public async void CreateBarcodeCommand_WhenAppendEventsFail_ShouldFail()
    {
        // arrange
        var cmd = BarcodeFaker.GetCreateBarcodeCommand();
        var item = BarcodeFaker.GetBarcodeRecord(cmd);
        _validatorMock
            .Setup(x => x.Validate(cmd))
            .Returns(new ValidationResult());
        _eventRepositoryMock
            .Setup(x => x.AppendEventsAsync(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<IEnumerable<IEvent>>()))
            .Returns(Task.FromResult(false));
        _mapperMock
            .Setup(x => x.Map<BarcodeRecord>(It.IsAny<BarcodeEntity>()))
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
            x => x.Map<BarcodeRecord>(It.IsAny<BarcodeEntity>()),
            Times.Never);
    }
    
    [Fact]
    public async void CreateBarcodeCommand_WhenException_ShouldFail()
    {
        // arrange
        var cmd = BarcodeFaker.GetCreateBarcodeCommand();
        var item = BarcodeFaker.GetBarcodeRecord(cmd);
        _validatorMock
            .Setup(x => x.Validate(cmd))
            .Returns(new ValidationResult());
        _eventRepositoryMock
            .Setup(x => x.AppendEventsAsync(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<IEnumerable<IEvent>>()))
            .Throws(new Exception());
        _mapperMock
            .Setup(x => x.Map<BarcodeRecord>(It.IsAny<BarcodeEntity>()))
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
            x => x.Map<BarcodeRecord>(It.IsAny<BarcodeEntity>()),
            Times.Never);
    }
    private CreateBarcodeCommandHandler GetCommandHandler() =>
        new CreateBarcodeCommandHandler(
            _BarcodeUpdateConfigMock.Object,
            _eventRepositoryMock.Object,
            _daprServiceMock.Object,
            _userServiceMock.Object,
            _mapperMock.Object,
            _validatorMock.Object,
            _loggerMock.Object);
}