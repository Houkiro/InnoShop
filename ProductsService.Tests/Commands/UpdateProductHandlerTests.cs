using Moq;
using ProductsService.Application.Commands.UpdateProduct;
using ProductsService.Application.Interfaces;

namespace ProductsService.Tests.Commands
{
    public class UpdateProductHandlerTests
    {
        private readonly Mock<IProductService> _productServiceMock;
        private readonly Mock<ICurrentUserService> _currentUserServiceMock;
        private readonly UpdateProductHandler _handler;

        public UpdateProductHandlerTests()
        {
            _productServiceMock = new Mock<IProductService>();
            _currentUserServiceMock = new Mock<ICurrentUserService>();
            _handler = new UpdateProductHandler(_productServiceMock.Object, _currentUserServiceMock.Object);
        }

        [Fact]
        public async Task Handle_CallsUpdateProductAsync_WithCorrectParameters()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var productId = Guid.NewGuid();  
            var command = new UpdateProductCommand(
                productId,
                "Updated Product Name", 
                "Updated Product Description", 
                100.0m, 
                false 
            );

            _currentUserServiceMock.Setup(c => c.UserId).Returns(userId); 

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            _productServiceMock.Verify(
                ps => ps.UpdateProductAsync(command, userId),
                Times.Once,
                "UpdateProductAsync was not called with the correct parameters"
            );
        }

        [Fact]
        public async Task Handle_DoesNotThrowException_WhenCalledWithValidData()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var productId = Guid.NewGuid();
            var command = new UpdateProductCommand(
                productId,
                "Updated Product Name",
                "Updated Product Description",
                100.0m,
                false
            );

            _currentUserServiceMock.Setup(c => c.UserId).Returns(userId); 

            // Act & Assert
            await _handler.Handle(command, CancellationToken.None); 
        }
    }
}
