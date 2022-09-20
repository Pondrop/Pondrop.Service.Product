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
using Pondrop.Service.Product.Application.Interfaces;
using Pondrop.Service.Product.Application.Queries;
using Pondrop.Service.Product.Tests.Faker;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Pondrop.Service.Product.Api.Tests
{
    public class CategoryControllerTests
    {
        private readonly Mock<IMediator> _mediatorMock;
        private readonly Mock<IServiceBusService> _serviceBusServiceMock;
        private readonly Mock<IRebuildCheckpointQueueService> _rebuildMaterializeViewQueueServiceMock;
        private readonly Mock<ILogger<CategoryController>> _loggerMock;
        private readonly Mock<IOptions<CategorySearchIndexConfiguration>> _searchIdxConfigMock;

        public CategoryControllerTests()
        {
            _mediatorMock = new Mock<IMediator>();
            _serviceBusServiceMock = new Mock<IServiceBusService>();
            _rebuildMaterializeViewQueueServiceMock = new Mock<IRebuildCheckpointQueueService>();
            _loggerMock = new Mock<ILogger<CategoryController>>();
            _searchIdxConfigMock = new Mock<IOptions<CategorySearchIndexConfiguration>>();
        }

        [Fact]
        public async void GetAllCategories_ShouldReturnOkResult()
        {
            // arrange
            var items = CategoryFaker.GetCategoryEntities();
            _mediatorMock
                .Setup(x => x.Send(It.IsAny<GetAllCategoriesQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result<List<CategoryEntity>>.Success(items));
            var controller = GetController();

            // act
            var response = await controller.GetAllCategories();

            // assert
            Assert.IsType<OkObjectResult>(response);
            Assert.Equal(((ObjectResult)response).Value, items);
            _mediatorMock.Verify(x => x.Send(It.IsAny<GetAllCategoriesQuery>(), It.IsAny<CancellationToken>()), Times.Once());
        }
        
        [Fact]
        public async void GetAllCategories_ShouldReturnBadResult_WhenFailedResult()
        {
            // arrange
            var failedResult = Result<List<CategoryEntity>>.Error("Invalid result!");
            _mediatorMock
                .Setup(x => x.Send(It.IsAny<GetAllCategoriesQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(failedResult);
            var controller = GetController();

            // act
            var response = await controller.GetAllCategories();

            // assert
            Assert.IsType<BadRequestObjectResult>(response);
            Assert.Equal(((ObjectResult)response).Value, failedResult.ErrorMessage);
            _mediatorMock.Verify(x => x.Send(It.IsAny<GetAllCategoriesQuery>(), It.IsAny<CancellationToken>()), Times.Once());
        }
        
        [Fact]
        public async void GetCategoryById_ShouldReturnOkResult()
        {
            // arrange
            var item = CategoryFaker.GetCategoryEntities(1).Single();
            _mediatorMock
                .Setup(x => x.Send(It.Is<GetCategoryByIdQuery>(x => x.Id == item.Id), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result<CategoryEntity>.Success(item));
            var controller = GetController();
        
            // act
            var response = await controller.GetCategoryById(item.Id);
        
            // assert
            Assert.IsType<OkObjectResult>(response);
            Assert.Equal(((ObjectResult)response).Value, item);
            _mediatorMock.Verify(x => x.Send(It.Is<GetCategoryByIdQuery>(x => x.Id == item.Id), It.IsAny<CancellationToken>()), Times.Once());
        }
        
        [Fact]
        public async void GetCategoryById_ShouldReturnBadResult_WhenFailedResult()
        {
            // arrange
            var failedResult = Result<CategoryEntity>.Error("Invalid result!");
            _mediatorMock
                .Setup(x => x.Send(It.IsAny<GetCategoryByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(failedResult);
            var controller = GetController();
        
            // act
            var response = await controller.GetCategoryById(Guid.NewGuid());
        
            // assert
            Assert.IsType<BadRequestObjectResult>(response);
            Assert.Equal(((ObjectResult)response).Value, failedResult.ErrorMessage);
            _mediatorMock.Verify(x => x.Send(It.IsAny<GetCategoryByIdQuery>(), It.IsAny<CancellationToken>()), Times.Once());
        }
        
        [Fact]
        public async void CreateCategoryCommand_ShouldReturnOkResult()
        {
            // arrange
            var cmd = CategoryFaker.GetCreateCategoryCommand();
            var item = CategoryFaker.GetCategoryRecord(cmd);
            _mediatorMock
                .Setup(x => x.Send(cmd, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result<CategoryRecord>.Success(item));
            var controller = GetController();
        
            // act
            var response = await controller.CreateCategory(cmd);
        
            // assert
            Assert.IsType<ObjectResult>(response);
            Assert.Equal(((ObjectResult)response).StatusCode, StatusCodes.Status201Created);
            Assert.Equal(((ObjectResult)response).Value, item);
            _mediatorMock.Verify(x => x.Send(cmd, It.IsAny<CancellationToken>()), Times.Once());
            _serviceBusServiceMock.Verify(x => x.SendMessageAsync(It.Is<UpdateCheckpointByIdCommand>(c => c.Id == item.Id)));
        }
        
        [Fact]
        public async void CreateCategoryCommand_ShouldReturnBadResult_WhenFailedResult()
        {
            // arrange
            var failedResult = Result<CategoryRecord>.Error("Invalid result!");
            _mediatorMock
                .Setup(x => x.Send(It.IsAny<CreateCategoryCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(failedResult);
            var controller = GetController();
        
            // act
            var response = await controller.CreateCategory(new CreateCategoryCommand());
        
            // assert
            Assert.IsType<BadRequestObjectResult>(response);
            Assert.Equal(((ObjectResult)response).Value, failedResult.ErrorMessage);
            _mediatorMock.Verify(x => x.Send(It.IsAny<CreateCategoryCommand>(), It.IsAny<CancellationToken>()), Times.Once());
        }
        
        [Fact]
        public async void UpdateCategoryCommand_ShouldReturnOkResult()
        {
            // arrange
            var cmd = CategoryFaker.GetUpdateCategoryCommand();
            var item = CategoryFaker.GetCategoryRecord(cmd);
            _mediatorMock
                .Setup(x => x.Send(cmd, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result<CategoryRecord>.Success(item));
            var controller = GetController();
        
            // act
            var response = await controller.UpdateCategory(cmd);
        
            // assert
            Assert.IsType<OkObjectResult>(response);
            Assert.Equal(((OkObjectResult)response).Value, item);
            _mediatorMock.Verify(x => x.Send(cmd, It.IsAny<CancellationToken>()), Times.Once());
            _serviceBusServiceMock.Verify(x => x.SendMessageAsync(It.Is<UpdateCheckpointByIdCommand>(c => c.Id == item.Id)));
        }
        
        [Fact]
        public async void UpdateCategoryCommand_ShouldReturnBadResult_WhenFailedResult()
        {
            // arrange
            var failedResult = Result<CategoryRecord>.Error("Invalid result!");
            _mediatorMock
                .Setup(x => x.Send(It.IsAny<UpdateCategoryCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(failedResult);
            var controller = GetController();
        
            // act
            var response = await controller.UpdateCategory(new UpdateCategoryCommand());
        
            // assert
            Assert.IsType<BadRequestObjectResult>(response);
            Assert.Equal(((ObjectResult)response).Value, failedResult.ErrorMessage);
            _mediatorMock.Verify(x => x.Send(It.IsAny<UpdateCategoryCommand>(), It.IsAny<CancellationToken>()), Times.Once());
        }
        
        [Fact]
        public async void UpdateCheckpoint_ShouldReturnOkResult()
        {
            // arrange
            var cmd = new UpdateCategoryCheckpointByIdCommand() { Id = Guid.NewGuid() };
            var item = CategoryFaker.GetCategoryRecords(1).Single() with { Id = cmd.Id };
            _mediatorMock
                .Setup(x => x.Send(cmd, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result<CategoryRecord>.Success(item));
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
            var failedResult = Result<CategoryRecord>.Error("Invalid result!");
            _mediatorMock
                .Setup(x => x.Send(It.IsAny<UpdateCategoryCheckpointByIdCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(failedResult);
            var controller = GetController();
        
            // act
            var response = await controller.UpdateCheckpoint(new UpdateCategoryCheckpointByIdCommand());
        
            // assert
            Assert.IsType<BadRequestObjectResult>(response);
            Assert.Equal(((ObjectResult)response).Value, failedResult.ErrorMessage);
            _mediatorMock.Verify(x => x.Send(It.IsAny<UpdateCategoryCheckpointByIdCommand>(), It.IsAny<CancellationToken>()), Times.Once());
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
            _rebuildMaterializeViewQueueServiceMock.Verify(x => x.Queue(It.IsAny<RebuildCategoryCheckpointCommand>()), Times.Once());
        }
        
        private CategoryController GetController() =>
            new CategoryController(
                _mediatorMock.Object,
                _serviceBusServiceMock.Object,
                _rebuildMaterializeViewQueueServiceMock.Object,
                _searchIdxConfigMock.Object,
                _loggerMock.Object
            );
    }
}
