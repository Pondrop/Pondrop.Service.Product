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
using Pondrop.Service.ProductCategory.Api.Controllers;
using Pondrop.Service.ProductCategory.Domain.Models;

namespace Pondrop.Service.Product.Api.Tests
{
    public class ProductCategoryControllerTests
    {
        private readonly Mock<IMediator> _mediatorMock;
        private readonly Mock<IServiceBusService> _serviceBusServiceMock;
        private readonly Mock<IRebuildCheckpointQueueService> _rebuildMaterializeViewQueueServiceMock;
        private readonly Mock<ILogger<ProductCategoryController>> _loggerMock;
        private readonly Mock<IOptions<SearchIndexConfiguration>> _searchIdxConfigMock;

        public ProductCategoryControllerTests()
        {
            _mediatorMock = new Mock<IMediator>();
            _serviceBusServiceMock = new Mock<IServiceBusService>();
            _rebuildMaterializeViewQueueServiceMock = new Mock<IRebuildCheckpointQueueService>();
            _loggerMock = new Mock<ILogger<ProductCategoryController>>();
            _searchIdxConfigMock = new Mock<IOptions<SearchIndexConfiguration>>();
        }

        [Fact]
        public async void GetAllProductCategories_ShouldReturnOkResult()
        {
            // arrange
            var items = ProductCategoryFaker.GetProductCategoryEntities();
            _mediatorMock
                .Setup(x => x.Send(It.IsAny<GetAllProductCategoriesQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result<List<ProductCategoryEntity>>.Success(items));
            var controller = GetController();

            // act
            var response = await controller.GetAllProductCategories();

            // assert
            Assert.IsType<OkObjectResult>(response);
            Assert.Equal(((ObjectResult)response).Value, items);
            _mediatorMock.Verify(x => x.Send(It.IsAny<GetAllProductCategoriesQuery>(), It.IsAny<CancellationToken>()), Times.Once());
        }
        
        [Fact]
        public async void GetAllProductCategories_ShouldReturnBadResult_WhenFailedResult()
        {
            // arrange
            var failedResult = Result<List<ProductCategoryEntity>>.Error("Invalid result!");
            _mediatorMock
                .Setup(x => x.Send(It.IsAny<GetAllProductCategoriesQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(failedResult);
            var controller = GetController();

            // act
            var response = await controller.GetAllProductCategories();

            // assert
            Assert.IsType<BadRequestObjectResult>(response);
            Assert.Equal(((ObjectResult)response).Value, failedResult.ErrorMessage);
            _mediatorMock.Verify(x => x.Send(It.IsAny<GetAllProductCategoriesQuery>(), It.IsAny<CancellationToken>()), Times.Once());
        }
        
        [Fact]
        public async void GetProductCategoryById_ShouldReturnOkResult()
        {
            // arrange
            var item = ProductCategoryFaker.GetProductCategoryEntities(1).Single();
            _mediatorMock
                .Setup(x => x.Send(It.Is<GetProductCategoryByIdQuery>(x => x.Id == item.Id), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result<ProductCategoryEntity>.Success(item));
            var controller = GetController();
        
            // act
            var response = await controller.GetProductCategoryById(item.Id);
        
            // assert
            Assert.IsType<OkObjectResult>(response);
            Assert.Equal(((ObjectResult)response).Value, item);
            _mediatorMock.Verify(x => x.Send(It.Is<GetProductCategoryByIdQuery>(x => x.Id == item.Id), It.IsAny<CancellationToken>()), Times.Once());
        }
        
        [Fact]
        public async void GetProductCategoryById_ShouldReturnBadResult_WhenFailedResult()
        {
            // arrange
            var failedResult = Result<ProductCategoryEntity>.Error("Invalid result!");
            _mediatorMock
                .Setup(x => x.Send(It.IsAny<GetProductCategoryByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(failedResult);
            var controller = GetController();
        
            // act
            var response = await controller.GetProductCategoryById(Guid.NewGuid());
        
            // assert
            Assert.IsType<BadRequestObjectResult>(response);
            Assert.Equal(((ObjectResult)response).Value, failedResult.ErrorMessage);
            _mediatorMock.Verify(x => x.Send(It.IsAny<GetProductCategoryByIdQuery>(), It.IsAny<CancellationToken>()), Times.Once());
        }
        
        [Fact]
        public async void CreateProductCategoryCommand_ShouldReturnOkResult()
        {
            // arrange
            var cmd = ProductCategoryFaker.GetCreateProductCategoryCommand();
            var item = ProductCategoryFaker.GetProductCategoryRecord(cmd);
            _mediatorMock
                .Setup(x => x.Send(cmd, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result<ProductCategoryRecord>.Success(item));
            var controller = GetController();
        
            // act
            var response = await controller.CreateProductCategory(cmd);
        
            // assert
            Assert.IsType<ObjectResult>(response);
            Assert.Equal(((ObjectResult)response).StatusCode, StatusCodes.Status201Created);
            Assert.Equal(((ObjectResult)response).Value, item);
            _mediatorMock.Verify(x => x.Send(cmd, It.IsAny<CancellationToken>()), Times.Once());
            _serviceBusServiceMock.Verify(x => x.SendMessageAsync(It.Is<UpdateCheckpointByIdCommand>(c => c.Id == item.Id)));
        }
        
        [Fact]
        public async void CreateProductCategoryCommand_ShouldReturnBadResult_WhenFailedResult()
        {
            // arrange
            var failedResult = Result<ProductCategoryRecord>.Error("Invalid result!");
            _mediatorMock
                .Setup(x => x.Send(It.IsAny<CreateProductCategoryCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(failedResult);
            var controller = GetController();
        
            // act
            var response = await controller.CreateProductCategory(new CreateProductCategoryCommand());
        
            // assert
            Assert.IsType<BadRequestObjectResult>(response);
            Assert.Equal(((ObjectResult)response).Value, failedResult.ErrorMessage);
            _mediatorMock.Verify(x => x.Send(It.IsAny<CreateProductCategoryCommand>(), It.IsAny<CancellationToken>()), Times.Once());
        }
        
        [Fact]
        public async void UpdateProductCategoryCommand_ShouldReturnOkResult()
        {
            // arrange
            var cmd = ProductCategoryFaker.GetUpdateProductCategoryCommand();
            var item = ProductCategoryFaker.GetProductCategoryRecord(cmd);
            _mediatorMock
                .Setup(x => x.Send(cmd, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result<ProductCategoryRecord>.Success(item));
            var controller = GetController();
        
            // act
            var response = await controller.UpdateProductCategory(cmd);
        
            // assert
            Assert.IsType<OkObjectResult>(response);
            Assert.Equal(((OkObjectResult)response).Value, item);
            _mediatorMock.Verify(x => x.Send(cmd, It.IsAny<CancellationToken>()), Times.Once());
            _serviceBusServiceMock.Verify(x => x.SendMessageAsync(It.Is<UpdateCheckpointByIdCommand>(c => c.Id == item.Id)));
        }
        
        [Fact]
        public async void UpdateProductCategoryCommand_ShouldReturnBadResult_WhenFailedResult()
        {
            // arrange
            var failedResult = Result<ProductCategoryRecord>.Error("Invalid result!");
            _mediatorMock
                .Setup(x => x.Send(It.IsAny<UpdateProductCategoryCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(failedResult);
            var controller = GetController();
        
            // act
            var response = await controller.UpdateProductCategory(new UpdateProductCategoryCommand());
        
            // assert
            Assert.IsType<BadRequestObjectResult>(response);
            Assert.Equal(((ObjectResult)response).Value, failedResult.ErrorMessage);
            _mediatorMock.Verify(x => x.Send(It.IsAny<UpdateProductCategoryCommand>(), It.IsAny<CancellationToken>()), Times.Once());
        }
        
        [Fact]
        public async void UpdateCheckpoint_ShouldReturnOkResult()
        {
            // arrange
            var cmd = new UpdateProductCategoryCheckpointByIdCommand() { Id = Guid.NewGuid() };
            var item = ProductCategoryFaker.GetProductCategoryRecords(1).Single() with { Id = cmd.Id };
            _mediatorMock
                .Setup(x => x.Send(cmd, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result<ProductCategoryRecord>.Success(item));
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
            var failedResult = Result<ProductCategoryRecord>.Error("Invalid result!");
            _mediatorMock
                .Setup(x => x.Send(It.IsAny<UpdateProductCategoryCheckpointByIdCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(failedResult);
            var controller = GetController();
        
            // act
            var response = await controller.UpdateCheckpoint(new UpdateProductCategoryCheckpointByIdCommand());
        
            // assert
            Assert.IsType<BadRequestObjectResult>(response);
            Assert.Equal(((ObjectResult)response).Value, failedResult.ErrorMessage);
            _mediatorMock.Verify(x => x.Send(It.IsAny<UpdateProductCategoryCheckpointByIdCommand>(), It.IsAny<CancellationToken>()), Times.Once());
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
            _rebuildMaterializeViewQueueServiceMock.Verify(x => x.Queue(It.IsAny<RebuildProductCategoryCheckpointCommand>()), Times.Once());
        }
        
        private ProductCategoryController GetController() =>
            new ProductCategoryController(
                _mediatorMock.Object,
                _serviceBusServiceMock.Object,
                _rebuildMaterializeViewQueueServiceMock.Object,
                _searchIdxConfigMock.Object,
                _loggerMock.Object
            );
    }
}
