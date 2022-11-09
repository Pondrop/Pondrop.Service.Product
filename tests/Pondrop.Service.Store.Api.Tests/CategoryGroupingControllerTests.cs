using System.Threading;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Pondrop.Service.Product.Application.Models;
using Pondrop.Service.Product.Domain.Models;
using Moq;
using Pondrop.Service.Product.Api.Controllers;
using Pondrop.Service.Product.Api.Models;
using Pondrop.Service.Product.Api.Services;
using Pondrop.Service.Product.Application.Commands;
using Pondrop.Service.Product.Application.Queries;
using Pondrop.Service.Product.Tests.Faker;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using Pondrop.Service.Interfaces;

namespace Pondrop.Service.Product.Api.Tests
{
    public class CategoryGroupingControllerTests
    {
        private readonly Mock<IMediator> _mediatorMock;
        private readonly Mock<IServiceBusService> _serviceBusServiceMock;
        private readonly Mock<IRebuildCheckpointQueueService> _rebuildMaterializeViewQueueServiceMock;
        private readonly Mock<ILogger<CategoryGroupingController>> _loggerMock;
        private readonly Mock<IOptions<SearchIndexConfiguration>> _searchIdxConfigMock;

        public CategoryGroupingControllerTests()
        {
            _mediatorMock = new Mock<IMediator>();
            _serviceBusServiceMock = new Mock<IServiceBusService>();
            _rebuildMaterializeViewQueueServiceMock = new Mock<IRebuildCheckpointQueueService>();
            _loggerMock = new Mock<ILogger<CategoryGroupingController>>();
            _searchIdxConfigMock = new Mock<IOptions<SearchIndexConfiguration>>();
        }

        [Fact]
        public async void GetAllCategoryGroupings_ShouldReturnOkResult()
        {
            // arrange
            var items = CategoryGroupingFaker.GetCategoryGroupingViewRecords();
            _mediatorMock
                .Setup(x => x.Send(It.IsAny<GetAllCategoryGroupingsQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result<List<CategoryGroupingViewRecord>>.Success(items));
            var controller = GetController();

            // act
            var response = await controller.GetAllCategoryGroupings();

            // assert
            Assert.IsType<OkObjectResult>(response);
            Assert.Equal(((ObjectResult)response).Value, items);
            _mediatorMock.Verify(x => x.Send(It.IsAny<GetAllCategoryGroupingsQuery>(), It.IsAny<CancellationToken>()), Times.Once());
        }
        
        [Fact]
        public async void GetAllCategoryGroupings_ShouldReturnBadResult_WhenFailedResult()
        {
            // arrange
            var failedResult = Result<List<CategoryGroupingViewRecord>>.Error("Invalid result!");
            _mediatorMock
                .Setup(x => x.Send(It.IsAny<GetAllCategoryGroupingsQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(failedResult);
            var controller = GetController();

            // act
            var response = await controller.GetAllCategoryGroupings();

            // assert
            Assert.IsType<BadRequestObjectResult>(response);
            Assert.Equal(((ObjectResult)response).Value, failedResult.ErrorMessage);
            _mediatorMock.Verify(x => x.Send(It.IsAny<GetAllCategoryGroupingsQuery>(), It.IsAny<CancellationToken>()), Times.Once());
        }
        
        [Fact]
        public async void GetCategoryGroupingById_ShouldReturnOkResult()
        {
            // arrange
            var item = CategoryGroupingFaker.GetCategoryGroupingViewRecords(1).FirstOrDefault();
            _mediatorMock
                .Setup(x => x.Send(It.Is<GetCategoryGroupingByIdQuery>(x => x.Id == item.Id), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result<CategoryGroupingViewRecord>.Success(item));
            var controller = GetController();
        
            // act
            var response = await controller.GetCategoryGroupingById(item.Id);
        
            // assert
            Assert.IsType<OkObjectResult>(response);
            Assert.Equal(((ObjectResult)response).Value, item);
            _mediatorMock.Verify(x => x.Send(It.Is<GetCategoryGroupingByIdQuery>(x => x.Id == item.Id), It.IsAny<CancellationToken>()), Times.Once());
        }
        
        [Fact]
        public async void GetCategoryGroupingById_ShouldReturnBadResult_WhenFailedResult()
        {
            // arrange
            var failedResult = Result<CategoryGroupingViewRecord>.Error("Invalid result!");
            _mediatorMock
                .Setup(x => x.Send(It.IsAny<GetCategoryGroupingByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(failedResult);
            var controller = GetController();
        
            // act
            var response = await controller.GetCategoryGroupingById(Guid.NewGuid());
        
            // assert
            Assert.IsType<BadRequestObjectResult>(response);
            Assert.Equal(((ObjectResult)response).Value, failedResult.ErrorMessage);
            _mediatorMock.Verify(x => x.Send(It.IsAny<GetCategoryGroupingByIdQuery>(), It.IsAny<CancellationToken>()), Times.Once());
        }
        
        [Fact]
        public async void CreateCategoryGroupingCommand_ShouldReturnOkResult()
        {
            // arrange
            var cmd = CategoryGroupingFaker.GetCreateCategoryGroupingCommand();
            var item = CategoryGroupingFaker.GetCategoryGroupingRecord(cmd);
            _mediatorMock
                .Setup(x => x.Send(cmd, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result<CategoryGroupingRecord>.Success(item));
            var controller = GetController();
        
            // act
            var response = await controller.CreateCategoryGrouping(cmd);
        
            // assert
            Assert.IsType<ObjectResult>(response);
            Assert.Equal(((ObjectResult)response).StatusCode, StatusCodes.Status201Created);
            Assert.Equal(((ObjectResult)response).Value, item);
            _mediatorMock.Verify(x => x.Send(cmd, It.IsAny<CancellationToken>()), Times.Once());
            _serviceBusServiceMock.Verify(x => x.SendMessageAsync(It.Is<UpdateCheckpointByIdCommand>(c => c.Id == item.Id)));
        }
        
        [Fact]
        public async void CreateCategoryGroupingCommand_ShouldReturnBadResult_WhenFailedResult()
        {
            // arrange
            var failedResult = Result<CategoryGroupingRecord>.Error("Invalid result!");
            _mediatorMock
                .Setup(x => x.Send(It.IsAny<CreateCategoryGroupingCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(failedResult);
            var controller = GetController();
        
            // act
            var response = await controller.CreateCategoryGrouping(new CreateCategoryGroupingCommand());
        
            // assert
            Assert.IsType<BadRequestObjectResult>(response);
            Assert.Equal(((ObjectResult)response).Value, failedResult.ErrorMessage);
            _mediatorMock.Verify(x => x.Send(It.IsAny<CreateCategoryGroupingCommand>(), It.IsAny<CancellationToken>()), Times.Once());
        }
        
        [Fact]
        public async void UpdateCategoryGroupingCommand_ShouldReturnOkResult()
        {
            // arrange
            var cmd = CategoryGroupingFaker.GetUpdateCategoryGroupingCommand();
            var item = CategoryGroupingFaker.GetCategoryGroupingRecord(cmd);
            _mediatorMock
                .Setup(x => x.Send(cmd, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result<CategoryGroupingRecord>.Success(item));
            var controller = GetController();
        
            // act
            var response = await controller.UpdateCategoryGrouping(cmd);
        
            // assert
            Assert.IsType<OkObjectResult>(response);
            Assert.Equal(((OkObjectResult)response).Value, item);
            _mediatorMock.Verify(x => x.Send(cmd, It.IsAny<CancellationToken>()), Times.Once());
            _serviceBusServiceMock.Verify(x => x.SendMessageAsync(It.Is<UpdateCheckpointByIdCommand>(c => c.Id == item.Id)));
        }
        
        [Fact]
        public async void UpdateCategoryGroupingCommand_ShouldReturnBadResult_WhenFailedResult()
        {
            // arrange
            var failedResult = Result<CategoryGroupingRecord>.Error("Invalid result!");
            _mediatorMock
                .Setup(x => x.Send(It.IsAny<UpdateCategoryGroupingCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(failedResult);
            var controller = GetController();
        
            // act
            var response = await controller.UpdateCategoryGrouping(new UpdateCategoryGroupingCommand());
        
            // assert
            Assert.IsType<BadRequestObjectResult>(response);
            Assert.Equal(((ObjectResult)response).Value, failedResult.ErrorMessage);
            _mediatorMock.Verify(x => x.Send(It.IsAny<UpdateCategoryGroupingCommand>(), It.IsAny<CancellationToken>()), Times.Once());
        }
        
        [Fact]
        public async void UpdateCheckpoint_ShouldReturnOkResult()
        {
            // arrange
            var cmd = new UpdateCategoryGroupingCheckpointByIdCommand() { Id = Guid.NewGuid() };
            var item = CategoryGroupingFaker.GetCategoryGroupingRecords(1).Single() with { Id = cmd.Id };
            _mediatorMock
                .Setup(x => x.Send(cmd, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result<CategoryGroupingRecord>.Success(item));
            var controller = GetController();
        
            // act
            var response = await controller.UpdateCheckpoint(cmd);
        
            // assert
            Assert.IsType<OkObjectResult>(response);
            Assert.Equal(((ObjectResult)response).Value, item);
            _mediatorMock.Verify(x => x.Send(cmd, It.IsAny<CancellationToken>()), Times.Once());
        }
        
        [Fact]
        public async void UpdateCheckpoint_ShouldReturnBadResult_WhenFailedResult()
        {
            // arrange
            var failedResult = Result<CategoryGroupingRecord>.Error("Invalid result!");
            _mediatorMock
                .Setup(x => x.Send(It.IsAny<UpdateCategoryGroupingCheckpointByIdCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(failedResult);
            var controller = GetController();
        
            // act
            var response = await controller.UpdateCheckpoint(new UpdateCategoryGroupingCheckpointByIdCommand());
        
            // assert
            Assert.IsType<BadRequestObjectResult>(response);
            Assert.Equal(((ObjectResult)response).Value, failedResult.ErrorMessage);
            _mediatorMock.Verify(x => x.Send(It.IsAny<UpdateCategoryGroupingCheckpointByIdCommand>(), It.IsAny<CancellationToken>()), Times.Once());
        }
        
        [Fact]
        public async void RebuildCheckpoint_ShouldReturnAcceptedResult()
        {
            // arrange
            var controller = GetController();
        
            // act
            var response = controller.RebuildCheckpoint();
        
            // assert
            Assert.IsType<AcceptedResult>(response);
            _rebuildMaterializeViewQueueServiceMock.Verify(x => x.Queue(It.IsAny<RebuildCategoryGroupingCheckpointCommand>()), Times.Once());
        }
        
        private CategoryGroupingController GetController() =>
            new CategoryGroupingController(
                _mediatorMock.Object,
                _serviceBusServiceMock.Object,
                _rebuildMaterializeViewQueueServiceMock.Object,
                _searchIdxConfigMock.Object,
                _loggerMock.Object
            );
    }
}
