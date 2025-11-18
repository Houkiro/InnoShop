using Microsoft.Extensions.Configuration;
using Moq;
using Moq.Protected;
using ProductsService.Infrastructure.Services;
using System.Net;

namespace ProductsService.Tests.Products.Services
{
    public class ProductIntegrationServiceTests
    {
        private readonly Mock<IConfiguration> _configMock;
        private readonly Mock<HttpMessageHandler> _handlerMock;
        private readonly HttpClient _httpClient;
        private readonly ProductIntegrationService _service;
        private readonly Guid _userId = Guid.NewGuid();

        public ProductIntegrationServiceTests()
        {
            _configMock = new Mock<IConfiguration>();
            _configMock.Setup(c => c["ServiceAuth:Secret"]).Returns("SECRET123");

            _handlerMock = new Mock<HttpMessageHandler>();
            _httpClient = new HttpClient(_handlerMock.Object)
            {
                BaseAddress = new Uri("https://test.com")
            };

            _service = new ProductIntegrationService(_httpClient, _configMock.Object);
        }

        [Fact]
        public async Task HideProductsAsync_ShouldCallHttpWithCorrectUrlAndHeader()
        {
            // Arrange
            _handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.Method == HttpMethod.Post &&
                        req.RequestUri == new Uri($"https://test.com/api/internal/products/hide-by-user/{_userId}") &&
                        req.Headers.Contains("X-Service-Auth") &&
                        req.Headers.GetValues("X-Service-Auth").Contains("SECRET123")
                    ),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK))
                .Verifiable();

            // Act
            await _service.HideProductsAsync(_userId);

            // Assert
            _handlerMock.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            );
        }

        [Fact]
        public async Task RestoreProductsAsync_ShouldCallHttpWithCorrectUrlAndHeader()
        {
            // Arrange
            _handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.Method == HttpMethod.Post &&
                        req.RequestUri == new Uri($"https://test.com/api/internal/products/restore-by-user/{_userId}") &&
                        req.Headers.Contains("X-Service-Auth") &&
                        req.Headers.GetValues("X-Service-Auth").Contains("SECRET123")
                    ),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK))
                .Verifiable();

            // Act
            await _service.RestoreProductsAsync(_userId);

            // Assert
            _handlerMock.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            );
        }

        [Fact]
        public async Task HideProductsAsync_ShouldThrow_WhenSecretMissing()
        {
            // Arrange
            _configMock.Setup(c => c["ServiceAuth:Secret"]).Returns((string)null);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _service.HideProductsAsync(_userId));
        }

        [Fact]
        public async Task RestoreProductsAsync_ShouldThrow_WhenSecretMissing()
        {
            // Arrange
            _configMock.Setup(c => c["ServiceAuth:Secret"]).Returns((string)null);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _service.RestoreProductsAsync(_userId));
        }
    }
}
