using Moq;
using ProductsService.Application.Commands.CreateProduct;
using ProductsService.Application.Interfaces;
using ProductsService.Domain.Entities;

namespace ProductsService.Tests.Products.Commands
{
    public class CreateProductHandlerTests
    {
        private readonly Mock<IProductRepository> _repoMock;
        private readonly Mock<IUnitOfWork> _uowMock;
        private readonly Mock<ICurrentUserService> _currentUserMock;
        private readonly CreateProductHandler _handler;

        public CreateProductHandlerTests()
        {
            _repoMock = new Mock<IProductRepository>();
            _uowMock = new Mock<IUnitOfWork>();
            _currentUserMock = new Mock<ICurrentUserService>();

            _currentUserMock.Setup(c => c.UserId).Returns(Guid.NewGuid());

            _handler = new CreateProductHandler(
                _repoMock.Object,
                _uowMock.Object,
                _currentUserMock.Object
            );
        }

        [Fact]
        public async Task Handle_ShouldCreateProductSuccessfully()
        {
            // Arrange
            var command = new CreateProductCommand(
                "Test Product",      
                "Test Description",  
                99.99m,              
                true                 
            );

            Product? addedProduct = null;
            _repoMock.Setup(r => r.AddAsync(It.IsAny<Product>()))
                     .Callback<Product>(p => addedProduct = p)
                     .Returns(Task.CompletedTask);

            _uowMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(0);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.NotEqual(Guid.Empty, result); // Проверяем, что Id не пустой
            Assert.NotNull(addedProduct);
            Assert.Equal(_currentUserMock.Object.UserId, addedProduct!.UserId);
            Assert.Equal(command.Title, addedProduct.Title);
            Assert.Equal(command.Description, addedProduct.Description);
            Assert.Equal(command.Price, addedProduct.Price);
            Assert.Equal(command.IsAvailable, addedProduct.IsAvailable);
            Assert.True(addedProduct.CreatedAt <= DateTime.UtcNow);

            _repoMock.Verify(r => r.AddAsync(It.IsAny<Product>()), Times.Once);
            _uowMock.Verify(u => u.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldThrowException_WhenRepositoryFails()
        {
            // Arrange
            var command = new CreateProductCommand(
                "Test Product",
                "Test Description",
                50m,
                true
            );

            _repoMock.Setup(r => r.AddAsync(It.IsAny<Product>()))
                     .ThrowsAsync(new Exception("Database error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() =>
                _handler.Handle(command, CancellationToken.None)
            );

            _repoMock.Verify(r => r.AddAsync(It.IsAny<Product>()), Times.Once);
            _uowMock.Verify(u => u.SaveChangesAsync(), Times.Never);
        }

        [Fact]
        public async Task Handle_ShouldUseCurrentUserId()
        {
            // Arrange
            var userId = Guid.NewGuid();
            _currentUserMock.Setup(c => c.UserId).Returns(userId);

            var command = new CreateProductCommand(
                "Title",
                "Desc",
                10m,
                false
            );

            Product? addedProduct = null;
            _repoMock.Setup(r => r.AddAsync(It.IsAny<Product>()))
                     .Callback<Product>(p => addedProduct = p)
                     .Returns(Task.CompletedTask);

            _uowMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(0);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.Equal(userId, addedProduct!.UserId);
        }

        [Fact]
        public async Task Handle_ShouldSetCreatedAtToUtcNow()
        {
            // Arrange
            var now = DateTime.UtcNow;

            var command = new CreateProductCommand(
                "Product",
                "Desc",
                20m,
                true
            );

            Product? addedProduct = null;
            _repoMock.Setup(r => r.AddAsync(It.IsAny<Product>()))
                     .Callback<Product>(p => addedProduct = p)
                     .Returns(Task.CompletedTask);

            _uowMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(0);

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(addedProduct!.CreatedAt >= now);
            Assert.True(addedProduct.CreatedAt <= DateTime.UtcNow);
        }
    }
}