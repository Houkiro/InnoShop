using Moq;
using ProductsService.Application.Commands.RestoreProductsByUser;
using ProductsService.Application.Interfaces;
using ProductsService.Domain.Entities;

namespace ProductsService.Tests.Commands
{
    public class RestoreProductsByUserHandlerTests
    {
        private readonly Mock<IProductRepository> _repoMock;
        private readonly Mock<IUnitOfWork> _uowMock;
        private readonly RestoreProductsByUserHandler _handler;

        public RestoreProductsByUserHandlerTests()
        {
            _repoMock = new Mock<IProductRepository>();
            _uowMock = new Mock<IUnitOfWork>();
            _handler = new RestoreProductsByUserHandler(_repoMock.Object, _uowMock.Object);
        }

        [Fact]
        public async Task Handle_RestoresDeletedProducts_WhenCalledWithValidUserId()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var products = new List<Product>
        {
            new Product { Id = Guid.NewGuid(), IsDeleted = true },
            new Product { Id = Guid.NewGuid(), IsDeleted = true }
        };

            _repoMock.Setup(r => r.GetByUserIdAsync(userId)).ReturnsAsync(products);

            var command = new RestoreProductsByUserCommand(userId); 

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            _repoMock.Verify(r => r.GetByUserIdAsync(userId), Times.Once);
            Assert.All(products, product => Assert.False(product.IsDeleted));
            _uowMock.Verify(u => u.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task Handle_DoesNotRestoreProducts_WhenNoProductsFound()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var products = new List<Product>();

            _repoMock.Setup(r => r.GetByUserIdAsync(userId)).ReturnsAsync(products);

            var command = new RestoreProductsByUserCommand(userId);

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            _repoMock.Verify(r => r.GetByUserIdAsync(userId), Times.Once);
            Assert.Empty(products);
            _uowMock.Verify(u => u.SaveChangesAsync(), Times.Never);
        }

        [Fact]
        public async Task Handle_ThrowsException_WhenRepositoryFailsToFetchProducts()
        {
            // Arrange
            var userId = Guid.NewGuid();
            _repoMock.Setup(r => r.GetByUserIdAsync(userId)).ThrowsAsync(new Exception("Repository failure"));

            var command = new RestoreProductsByUserCommand(userId); 

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => _handler.Handle(command, CancellationToken.None));
            Assert.Equal("Repository failure", exception.Message);
        }
    }
}
