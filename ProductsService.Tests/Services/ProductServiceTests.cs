using LoggingService;
using Moq;
using ProductsService.Application.Commands.CreateProduct;
using ProductsService.Application.Commands.UpdateProduct;
using ProductsService.Application.Interfaces;
using ProductsService.Domain.Entities;
using ProductsService.Domain.Exceptions;
using ProductsService.Infrastructure.Services;
using Xunit;

namespace ProductsService.Tests.Infrastructure.Services
{
    public class ProductServiceTests
    {
        private readonly Mock<IProductRepository> _repoMock;
        private readonly Mock<IUnitOfWork> _uowMock;
        private readonly Mock<ILoggingService> _loggerMock;
        private readonly ProductService _service;

        public ProductServiceTests()
        {
            _repoMock = new Mock<IProductRepository>();
            _uowMock = new Mock<IUnitOfWork>();
            _loggerMock = new Mock<ILoggingService>();
            _service = new ProductService(_repoMock.Object, _uowMock.Object, _loggerMock.Object);
        }

        [Fact]
        public async Task CreateProductAsync_AddsProduct_AndSaves()
        {
            var command = new CreateProductCommand("Title", "Desc", 100m, true);
            var userId = Guid.NewGuid();

            var result = await _service.CreateProductAsync(command, userId);

            _repoMock.Verify(r => r.AddAsync(It.Is<Product>(p =>
                p.Title == "Title" &&
                p.Description == "Desc" &&
                p.Price == 100m &&
                p.IsAvailable == true &&
                p.UserId == userId)), Times.Once);

            _uowMock.Verify(u => u.SaveChangesAsync(), Times.Once);
            Assert.NotEqual(Guid.Empty, result);
        }

        [Fact]
        public async Task UpdateProductAsync_Throws_WhenProductNotFound()
        {
            var command = new UpdateProductCommand(Guid.NewGuid(), "NewTitle", "NewDesc", 200m, true);
            _repoMock.Setup(r => r.GetByIdAsync(command.ProductId)).ReturnsAsync((Product?)null);

            await Assert.ThrowsAsync<ProductNotFoundException>(() =>
                _service.UpdateProductAsync(command, Guid.NewGuid()));
        }

        [Fact]
        public async Task UpdateProductAsync_Throws_WhenUserMismatch()
        {
            var product = new Product { Id = Guid.NewGuid(), UserId = Guid.NewGuid() };
            var command = new UpdateProductCommand(product.Id, "NewTitle", "NewDesc", 200m, true);

            _repoMock.Setup(r => r.GetByIdAsync(product.Id)).ReturnsAsync(product);

            await Assert.ThrowsAsync<AccessDeniedException>(() =>
                _service.UpdateProductAsync(command, Guid.NewGuid()));
        }

        [Fact]
        public async Task UpdateProductAsync_UpdatesProduct_WhenValid()
        {
            var userId = Guid.NewGuid();
            var product = new Product { Id = Guid.NewGuid(), UserId = userId };
            var command = new UpdateProductCommand(product.Id, "NewTitle", "NewDesc", 200m, true);

            _repoMock.Setup(r => r.GetByIdAsync(product.Id)).ReturnsAsync(product);

            await _service.UpdateProductAsync(command, userId);

            _repoMock.Verify(r => r.UpdateAsync(It.Is<Product>(p =>
                p.Title == "NewTitle" &&
                p.Description == "NewDesc" &&
                p.Price == 200m &&
                p.IsAvailable == true)), Times.Once);

            _uowMock.Verify(u => u.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task DeleteProductAsync_Throws_WhenProductNotFound()
        {
            var productId = Guid.NewGuid();
            _repoMock.Setup(r => r.GetByIdAsync(productId)).ReturnsAsync((Product?)null);

            await Assert.ThrowsAsync<ProductNotFoundException>(() =>
                _service.DeleteProductAsync(productId, Guid.NewGuid()));
        }

        [Fact]
        public async Task DeleteProductAsync_Throws_WhenUserMismatch()
        {
            var product = new Product { Id = Guid.NewGuid(), UserId = Guid.NewGuid() };
            _repoMock.Setup(r => r.GetByIdAsync(product.Id)).ReturnsAsync(product);

            await Assert.ThrowsAsync<AccessDeniedException>(() =>
                _service.DeleteProductAsync(product.Id, Guid.NewGuid()));
        }

        [Fact]
        public async Task DeleteProductAsync_SetsIsDeleted_WhenValid()
        {
            var userId = Guid.NewGuid();
            var product = new Product { Id = Guid.NewGuid(), UserId = userId };
            _repoMock.Setup(r => r.GetByIdAsync(product.Id)).ReturnsAsync(product);

            await _service.DeleteProductAsync(product.Id, userId);

            _repoMock.Verify(r => r.UpdateAsync(It.Is<Product>(p => p.IsDeleted)), Times.Once);
            _uowMock.Verify(u => u.SaveChangesAsync(), Times.Once);
        }
    }
}