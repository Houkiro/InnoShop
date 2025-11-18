using Moq;
using ProductsService.Application.Commands.UpdateProduct;
using ProductsService.Application.Interfaces;
using ProductsService.Domain.Entities;

namespace ProductsService.Tests.Products.Commands
{
    public class UpdateProductHandlerTests
    {
        private readonly Mock<IProductRepository> _repoMock;
        private readonly Mock<IUnitOfWork> _uowMock;
        private readonly Mock<ICurrentUserService> _currentUserMock;
        private readonly UpdateProductHandler _handler;

        public UpdateProductHandlerTests()
        {
            _repoMock = new Mock<IProductRepository>();
            _uowMock = new Mock<IUnitOfWork>();
            _currentUserMock = new Mock<ICurrentUserService>();

            _handler = new UpdateProductHandler(
                _repoMock.Object,
                _uowMock.Object,
                _currentUserMock.Object
            );
        }

        [Fact]
        public async Task Handle_ShouldUpdateProduct_WhenUserOwnsProduct()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var productId = Guid.NewGuid();
            _currentUserMock.Setup(c => c.UserId).Returns(userId);

            var existingProduct = new Product
            {
                Id = productId,
                UserId = userId,
                Title = "Old Title",
                Description = "Old Description",
                Price = 10m,
                IsAvailable = false
            };

            _repoMock.Setup(r => r.GetByIdAsync(productId)).ReturnsAsync(existingProduct);
            _repoMock.Setup(r => r.UpdateAsync(existingProduct)).Returns(Task.CompletedTask);
            _uowMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(0);

            var command = new UpdateProductCommand(
                ProductId: productId,
                Title: "New Title",
                Description: "New Description",
                Price: 20m,
                IsAvailable: true
            );

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.Equal("New Title", existingProduct.Title);
            Assert.Equal("New Description", existingProduct.Description);
            Assert.Equal(20m, existingProduct.Price);
            Assert.True(existingProduct.IsAvailable);

            _repoMock.Verify(r => r.UpdateAsync(existingProduct), Times.Once);
            _uowMock.Verify(u => u.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldThrowKeyNotFoundException_WhenProductDoesNotExist()
        {
            // Arrange
            var productId = Guid.NewGuid();
            _repoMock.Setup(r => r.GetByIdAsync(productId)).ReturnsAsync((Product?)null);

            var command = new UpdateProductCommand(
                ProductId: productId,
                Title: "New Title",
                Description: "New Description",
                Price: 20m,
                IsAvailable: true
            );

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                _handler.Handle(command, CancellationToken.None));
        }

        [Fact]
        public async Task Handle_ShouldThrowUnauthorizedAccessException_WhenUserDoesNotOwnProduct()
        {
            // Arrange
            var productId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            _currentUserMock.Setup(c => c.UserId).Returns(userId);

            var existingProduct = new Product
            {
                Id = productId,
                UserId = Guid.NewGuid(), 
                Title = "Old Title",
                Description = "Old Description",
                Price = 10m,
                IsAvailable = false
            };

            _repoMock.Setup(r => r.GetByIdAsync(productId)).ReturnsAsync(existingProduct);

            var command = new UpdateProductCommand(
                ProductId: productId,
                Title: "New Title",
                Description: "New Description",
                Price: 20m,
                IsAvailable: true
            );

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                _handler.Handle(command, CancellationToken.None));
        }
    }
}
