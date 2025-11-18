using Microsoft.Extensions.Configuration;
using Moq;
using Moq.Protected;
using System.Net;
using UsersService.Application.Services;

namespace UsersService.Tests.Users.Services
{
    public class ProductIntegrationServiceTests
    {
        private HttpClient CreateHttpClient(Mock<HttpMessageHandler> handlerMock)
        {
            return new HttpClient(handlerMock.Object)
            {
                BaseAddress = new Uri("https://fake.api/")
            };
        }

        private IConfiguration CreateConfig()
        {
            var settings = new Dictionary<string, string?>
        {
            { "ServiceAuth:Secret", "test-secret" }
        };

            return new ConfigurationBuilder()
                .AddInMemoryCollection(settings)
                .Build();
        }

        [Fact]
        public async Task HideProductsAsync_ShouldSendCorrectRequest_AndSetAuthHeader()
        {
            // Arrange
            var userId = Guid.NewGuid();

            var handlerMock = new Mock<HttpMessageHandler>();
            HttpRequestMessage? capturedRequest = null;

            handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .Callback<HttpRequestMessage, CancellationToken>((req, _) => capturedRequest = req)
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

            var httpClient = CreateHttpClient(handlerMock);
            var config = CreateConfig();

            var service = new ProductIntegrationService(httpClient, config);

            // Act
            await service.HideProductsAsync(userId);

            // Assert
            Assert.NotNull(capturedRequest);
            Assert.Equal(HttpMethod.Post, capturedRequest!.Method);
            Assert.Equal($"https://fake.api/internal/hide-products/{userId}",
                         capturedRequest!.RequestUri!.ToString());

            Assert.True(capturedRequest.Headers.Contains("X-Service-Auth"));
            Assert.Equal("test-secret",
                capturedRequest.Headers.GetValues("X-Service-Auth").First());
        }

        [Fact]
        public async Task RestoreProductsAsync_ShouldSendCorrectRequest_AndSetAuthHeader()
        {
            // Arrange
            var userId = Guid.NewGuid();

            var handlerMock = new Mock<HttpMessageHandler>();
            HttpRequestMessage? capturedRequest = null;

            handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .Callback<HttpRequestMessage, CancellationToken>((req, _) => capturedRequest = req)
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

            var httpClient = CreateHttpClient(handlerMock);
            var config = CreateConfig();

            var service = new ProductIntegrationService(httpClient, config);

            // Act
            await service.RestoreProductsAsync(userId);

            // Assert
            Assert.NotNull(capturedRequest);
            Assert.Equal(HttpMethod.Post, capturedRequest!.Method);
            Assert.Equal($"https://fake.api/internal/restore-products/{userId}",
                         capturedRequest!.RequestUri!.ToString());

            Assert.True(capturedRequest.Headers.Contains("X-Service-Auth"));
        }

        [Fact]
        public async Task HideProductsAsync_ShouldThrow_WhenServerReturnsError()
        {
            // Arrange
            var handlerMock = new Mock<HttpMessageHandler>();

            handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.BadRequest));

            var httpClient = CreateHttpClient(handlerMock);
            var config = CreateConfig();

            var service = new ProductIntegrationService(httpClient, config);

            // Act & Assert
            await Assert.ThrowsAsync<HttpRequestException>(() =>
                service.HideProductsAsync(Guid.NewGuid()));
        }
    }
}
