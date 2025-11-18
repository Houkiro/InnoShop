using Moq;
using ProductsService.Application.Commands.RestoreProductsByUser;
using ProductsService.Application.Interfaces;
using ProductsService.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductsService.Tests.Products.Commands
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
        public async Task Handle_ShouldRestoreAllProductsForUser()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var products = new List<Product>
        {
            new Product { Id = Guid.NewGuid(), UserId = userId, IsDeleted = true },
            new Product { Id = Guid.NewGuid(), UserId = userId, IsDeleted = true }
        };

            _repoMock.Setup(r => r.GetByUserIdAsync(userId)).ReturnsAsync(products);
            _uowMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(0);

            var command = new RestoreProductsByUserCommand(userId);

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            foreach (var product in products)
            {
                Assert.False(product.IsDeleted);
            }

            _uowMock.Verify(u => u.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldDoNothing_WhenUserHasNoProducts()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var products = new List<Product>();

            _repoMock.Setup(r => r.GetByUserIdAsync(userId)).ReturnsAsync(products);
            _uowMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(0);

            var command = new RestoreProductsByUserCommand(userId);

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.Empty(products);
            _uowMock.Verify(u => u.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldNotChangeAlreadyActiveProducts()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var products = new List<Product>
        {
            new Product { Id = Guid.NewGuid(), UserId = userId, IsDeleted = false },
            new Product { Id = Guid.NewGuid(), UserId = userId, IsDeleted = true }
        };

            _repoMock.Setup(r => r.GetByUserIdAsync(userId)).ReturnsAsync(products);
            _uowMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(0);

            var command = new RestoreProductsByUserCommand(userId);

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.All(products, p => Assert.False(p.IsDeleted));
            _uowMock.Verify(u => u.SaveChangesAsync(), Times.Once);
        }
    }
}
