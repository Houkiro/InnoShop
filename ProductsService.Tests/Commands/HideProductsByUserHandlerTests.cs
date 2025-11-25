using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using ProductsService.Application.Commands.HideProductsByUser;
using ProductsService.Application.Interfaces;
using ProductsService.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace ProductsService.Tests.Commands
{
    public class HideProductsByUserHandlerTests
    {
        private readonly Mock<IProductRepository> _repoMock;
        private readonly Mock<IUnitOfWork> _uowMock;
        private readonly Mock<ILogger<HideProductsByUserHandler>> _loggerMock;
        private readonly HideProductsByUserHandler _handler;

        public HideProductsByUserHandlerTests()
        {
            _repoMock = new Mock<IProductRepository>();
            _uowMock = new Mock<IUnitOfWork>();
            _loggerMock = new Mock<ILogger<HideProductsByUserHandler>>();
            _handler = new HideProductsByUserHandler(_repoMock.Object, _uowMock.Object, _loggerMock.Object);
        }

        [Fact]
        public async Task Handle_ShouldMarkProductsAsDeleted_AndCallSaveChanges()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var products = new List<Product>
            {
                new Product { Id = Guid.NewGuid(), IsDeleted = false },
                new Product { Id = Guid.NewGuid(), IsDeleted = false }
            };

            _repoMock.Setup(r => r.GetByUserIdAsync(userId))
                     .ReturnsAsync(products);

            var command = new HideProductsByUserCommand(userId);

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.All(products, p => Assert.True(p.IsDeleted));
            _uowMock.Verify(u => u.SaveChangesAsync(), Times.Once);

            _loggerMock.Verify(
                l => l.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((o, t) => o.ToString().Contains("Hiding product")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Exactly(products.Count));
        }

        [Fact]
        public async Task Handle_ShouldCallRepositoryWithCorrectUserId()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var command = new HideProductsByUserCommand(userId);

            _repoMock.Setup(r => r.GetByUserIdAsync(userId))
                     .ReturnsAsync(new List<Product>());

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            _repoMock.Verify(r => r.GetByUserIdAsync(userId), Times.Once);
        }
    }
}