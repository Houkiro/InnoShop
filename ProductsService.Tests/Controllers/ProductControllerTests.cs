using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using ProductsService.Application.Commands.CreateProduct;
using ProductsService.Application.Commands.DeleteProduct;
using ProductsService.Application.Commands.UpdateProduct;
using ProductsService.Application.Contracts;
using ProductsService.Application.Queries.GetProductById.Dto;
using ProductsService.Application.Queries.GetProducts;
using ProductsService.Controllers;
using System.Security.Claims;

namespace ProductsService.Tests.Controllers
{
    public class ProductControllerTests
    {
        private readonly Mock<IMediator> _mediatorMock;
        private readonly ProductController _controller;

        public ProductControllerTests()
        {
            _mediatorMock = new Mock<IMediator>();
            _controller = new ProductController(_mediatorMock.Object);
        }

        [Fact]
        public async Task Create_ShouldSendCommand_AndReturnCreatedAtAction()
        {
            // Arrange
            var command = new CreateProductCommand("Title", "Desc", 10, true);
            var newId = Guid.NewGuid();
            _mediatorMock
                .Setup(m => m.Send(command, It.IsAny<CancellationToken>()))
                .ReturnsAsync(newId);

            // Act
            var result = await _controller.Create(command);

            // Assert
            var created = Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal(nameof(ProductController.GetById), created.ActionName);
            Assert.Equal(newId, created.RouteValues["id"]);
        }

        [Fact]
        public async Task GetById_ShouldSendQuery_AndReturnOk()
        {
            // Arrange
            var id = Guid.NewGuid();
            var dto = new ProductDto { Id = id, Title = "Test" };
            _mediatorMock
                .Setup(m => m.Send(It.Is<GetProductByIdQuery>(q => q.Id == id), It.IsAny<CancellationToken>()))
                .ReturnsAsync(dto);

            // Act
            var result = await _controller.GetById(id);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(dto, ok.Value);
        }

        [Fact]
        public async Task Update_ShouldSendCommandWithId_AndReturnNoContent()
        {
            // Arrange
            var id = Guid.NewGuid();
            var command = new UpdateProductCommand(id, "Title", "Desc", 10, true);
            _mediatorMock
                .Setup(m => m.Send(It.Is<UpdateProductCommand>(c => c.ProductId == id), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.Update(id, command);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task Delete_ShouldSendCommand_AndReturnNoContent()
        {
            var id = Guid.NewGuid();
            var userId = Guid.NewGuid();

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(
                        new ClaimsIdentity(
                        [
                            new Claim(ClaimTypes.NameIdentifier, userId.ToString())
                        ])
                    )
                }
            };

            _mediatorMock
                .Setup(m => m.Send(It.Is<DeleteProductCommand>(c => c.ProductId == id && c.UserId == userId),
                                   It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await _controller.Delete(id);

            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task Get_ShouldSendQuery_AndReturnOk()
        {
            var query = new GetProductsQuery(null, null, null, null, 1, 10, null, false);
            var products = new[] { new ProductDto { Id = Guid.NewGuid(), Title = "Test" } };

            _mediatorMock
                .Setup(m => m.Send(It.IsAny<GetProductsQuery>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(products.ToList())); 
                                                            
            var result = await _controller.Get(query);

            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(products, ok.Value);
        }


    }
}
