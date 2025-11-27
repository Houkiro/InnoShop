using Moq;
using ProductsService.Application.Commands.CreateProduct;
using ProductsService.Application.Interfaces;

namespace ProductsService.Tests.Commands
{
    public class CreateProductHandlerTests
    {
        [Fact]
        public async Task Handle_ShouldReturnGuid_WhenProductCreated()
        {
            var productServiceMock = new Mock<IProductService>();
            var currentUserMock = new Mock<ICurrentUserService>();

            var userId = Guid.NewGuid();
            var productId = Guid.NewGuid();

            currentUserMock.Setup(c => c.UserId).Returns(userId);

            var command = new CreateProductCommand("Title", "Description", 100m, true);

            productServiceMock
                .Setup(s => s.CreateProductAsync(command, userId))
                .ReturnsAsync(productId);

            var handler = new CreateProductHandler(productServiceMock.Object, currentUserMock.Object);

            var result = await handler.Handle(command, CancellationToken.None);

            Assert.Equal(productId, result);
            productServiceMock.Verify(s => s.CreateProductAsync(command, userId), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldPassCorrectUserIdToService()
        {
            var productServiceMock = new Mock<IProductService>();
            var currentUserMock = new Mock<ICurrentUserService>();

            var userId = Guid.NewGuid();
            currentUserMock.Setup(c => c.UserId).Returns(userId);

            var command = new CreateProductCommand("Title", "Description", 50m, false);

            var handler = new CreateProductHandler(productServiceMock.Object, currentUserMock.Object);

            await handler.Handle(command, CancellationToken.None);

            productServiceMock.Verify(s => s.CreateProductAsync(command, userId), Times.Once);
        }
    }
}
