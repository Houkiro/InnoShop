using Moq;
using ProductsService.Application.Commands.DeleteProduct;
using ProductsService.Application.Interfaces;
using ProductsService.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductsService.Tests.Products.Commands
{
    public class DeleteProductHandlerTests
    {
        private readonly Mock<IProductRepository> _repoMock;
        private readonly Mock<IUnitOfWork> _uowMock;
        private readonly DeleteProductHandler _handler;

        public DeleteProductHandlerTests()
        {
            _repoMock = new Mock<IProductRepository>();
            _uowMock = new Mock<IUnitOfWork>();

            _handler = new DeleteProductHandler(_repoMock.Object, _uowMock.Object);
        }

        [Fact]
        public async Task Handle_ShouldDeleteProductSuccessfully()
        {
            // Arrange
            var productId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var product = new Product
            {
                Id = productId,
                UserId = userId,
                IsDeleted = false
            };

            _repoMock.Setup(r => r.GetByIdAsync(productId))
                     .ReturnsAsync(product);

            _repoMock.Setup(r => r.UpdateAsync(It.IsAny<Product>()))
                     .Returns(Task.CompletedTask);

            _uowMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(0);

            var command = new DeleteProductCommand(productId, userId);

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(product.IsDeleted);
            _repoMock.Verify(r => r.UpdateAsync(product), Times.Once);
            _uowMock.Verify(u => u.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldThrowKeyNotFoundException_WhenProductNotFound()
        {
            // Arrange
            var productId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            _repoMock.Setup(r => r.GetByIdAsync(productId))
                     .ReturnsAsync((Product?)null);

            var command = new DeleteProductCommand(productId, userId);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                _handler.Handle(command, CancellationToken.None)
            );

            _repoMock.Verify(r => r.UpdateAsync(It.IsAny<Product>()), Times.Never);
            _uowMock.Verify(u => u.SaveChangesAsync(), Times.Never);
        }

        [Fact]
        public async Task Handle_ShouldThrowUnauthorizedAccessException_WhenUserIsNotOwner()
        {
            // Arrange
            var productId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var product = new Product
            {
                Id = productId,
                UserId = Guid.NewGuid(), // другой пользователь
                IsDeleted = false
            };

            _repoMock.Setup(r => r.GetByIdAsync(productId))
                     .ReturnsAsync(product);

            var command = new DeleteProductCommand(productId, userId);

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                _handler.Handle(command, CancellationToken.None)
            );

            _repoMock.Verify(r => r.UpdateAsync(It.IsAny<Product>()), Times.Never);
            _uowMock.Verify(u => u.SaveChangesAsync(), Times.Never);
        }

        [Fact]
        public async Task Handle_ShouldNotDeleteAlreadyDeletedProduct()
        {
            // Arrange
            var productId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var product = new Product
            {
                Id = productId,
                UserId = userId,
                IsDeleted = true
            };

            _repoMock.Setup(r => r.GetByIdAsync(productId))
                     .ReturnsAsync(product);

            _repoMock.Setup(r => r.UpdateAsync(It.IsAny<Product>()))
                     .Returns(Task.CompletedTask);

            _uowMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(0);

            var command = new DeleteProductCommand(productId, userId);

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(product.IsDeleted);
            _repoMock.Verify(r => r.UpdateAsync(product), Times.Once);
            _uowMock.Verify(u => u.SaveChangesAsync(), Times.Once);
        }
    }
}
