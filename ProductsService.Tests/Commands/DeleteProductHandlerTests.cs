using Moq;
using ProductsService.Application.Commands.DeleteProduct;
using ProductsService.Application.Interfaces;

namespace ProductsService.Tests.Commands
{
    public class DeleteProductHandlerTests
    {
        [Fact]
        public async Task Handle_ShouldCallDeleteProductAsync_WithCorrectArguments()
        {
            // Arrange
            var productServiceMock = new Mock<IProductService>();
            var currentUserMock = new Mock<ICurrentUserService>();

            var userId = Guid.NewGuid();
            var productId = Guid.NewGuid();

            currentUserMock.Setup(c => c.UserId).Returns(userId);

            var command = new DeleteProductCommand(productId, userId);
            var handler = new DeleteProductHandler(productServiceMock.Object, currentUserMock.Object);

            // Act
            await handler.Handle(command, CancellationToken.None);

            // Assert
            productServiceMock.Verify(
                s => s.DeleteProductAsync(productId, userId),
                Times.Once
            );
        }

        [Fact]
        public async Task Handle_ShouldUseUserIdFromCurrentUserService()
        {
            // Arrange
            var productServiceMock = new Mock<IProductService>();
            var currentUserMock = new Mock<ICurrentUserService>();

            var expectedUserId = Guid.NewGuid();
            var productId = Guid.NewGuid();

            currentUserMock.Setup(c => c.UserId).Returns(expectedUserId);

            var command = new DeleteProductCommand(productId, Guid.Empty);
            var handler = new DeleteProductHandler(productServiceMock.Object, currentUserMock.Object);

            // Act
            await handler.Handle(command, CancellationToken.None);

            // Assert
            productServiceMock.Verify(
                s => s.DeleteProductAsync(productId, expectedUserId),
                Times.Once
            );
        }
    }
}
