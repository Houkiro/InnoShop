using Moq;
using ProductsService.Application.Commands.HideProductsByUser;
using ProductsService.Application.Interfaces;
using ProductsService.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductsService.Tests.Products.Commands
{
    public class HideProductsByUserHandlerTests
    {
        private readonly Mock<IProductRepository> _repoMock;
        private readonly Mock<IUnitOfWork> _uowMock;
        private readonly HideProductsByUserHandler _handler;

        public HideProductsByUserHandlerTests()
        {
            _repoMock = new Mock<IProductRepository>();
            _uowMock = new Mock<IUnitOfWork>();
            _handler = new HideProductsByUserHandler(_repoMock.Object, _uowMock.Object);
        }

        [Fact]
        public async Task Handle_ShouldHideAllProductsForUser()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var products = new List<Product>
        {
            new Product { Id = Guid.NewGuid(), UserId = userId, IsDeleted = false },
            new Product { Id = Guid.NewGuid(), UserId = userId, IsDeleted = false }
        };

            _repoMock.Setup(r => r.GetByUserIdAsync(userId))
                     .ReturnsAsync(products);

            _uowMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(0);

            var command = new HideProductsByUserCommand(userId);

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            foreach (var product in products)
            {
                Assert.True(product.IsDeleted);
            }

            _uowMock.Verify(u => u.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldDoNothing_WhenUserHasNoProducts()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var products = new List<Product>();

            _repoMock.Setup(r => r.GetByUserIdAsync(userId))
                     .ReturnsAsync(products);

            _uowMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(0);

            var command = new HideProductsByUserCommand(userId);

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.Empty(products);
            _uowMock.Verify(u => u.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldHideOnlyNonDeletedProducts()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var products = new List<Product>
        {
            new Product { Id = Guid.NewGuid(), UserId = userId, IsDeleted = false },
            new Product { Id = Guid.NewGuid(), UserId = userId, IsDeleted = true }
        };

            _repoMock.Setup(r => r.GetByUserIdAsync(userId))
                     .ReturnsAsync(products);

            _uowMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(0);

            var command = new HideProductsByUserCommand(userId);

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.All(products, p => Assert.True(p.IsDeleted));
            _uowMock.Verify(u => u.SaveChangesAsync(), Times.Once);
        }
    }
}