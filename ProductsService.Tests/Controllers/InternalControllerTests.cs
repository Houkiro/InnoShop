using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using ProductsService.Application.Commands.HideProductsByUser;
using ProductsService.Application.Commands.RestoreProductsByUser;
using ProductsService.Controllers;

namespace ProductsService.Tests.Controllers
{
    public class InternalControllerTests
    {
        private readonly Mock<IMediator> _mediatorMock;
        private readonly InternalController _controller;

        public InternalControllerTests()
        {
            _mediatorMock = new Mock<IMediator>();
            _controller = new InternalController(_mediatorMock.Object);
        }

        [Fact]
        public async Task HideProducts_ShouldSendCommand_AndReturnNoContent()
        {
            var userId = Guid.NewGuid();
            _mediatorMock
                .Setup(m => m.Send(It.IsAny<HideProductsByUserCommand>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await _controller.HideProducts(userId);

            _mediatorMock.Verify(m => m.Send(
                It.Is<HideProductsByUserCommand>(c => c.UserId == userId),
                It.IsAny<CancellationToken>()),
                Times.Once);

            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task RestoreProducts_ShouldSendCommand_AndReturnNoContent()
        {
            var userId = Guid.NewGuid();
            _mediatorMock
                .Setup(m => m.Send(It.IsAny<RestoreProductsByUserCommand>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await _controller.RestoreProducts(userId);

            _mediatorMock.Verify(m => m.Send(
                It.Is<RestoreProductsByUserCommand>(c => c.UserId == userId),
                It.IsAny<CancellationToken>()),
                Times.Once);

            Assert.IsType<NoContentResult>(result);
        }
    }
}