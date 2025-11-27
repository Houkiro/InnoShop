using Microsoft.Extensions.Configuration;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using UsersService.Application.Services;
using Xunit;

namespace UsersService.Tests.Services
{
    public class ProductIntegrationServiceTests
    {
        private class StubHandler : HttpMessageHandler
        {
            public HttpRequestMessage? LastRequest { get; private set; }
            private readonly HttpResponseMessage _response;

            public StubHandler(HttpResponseMessage response)
            {
                _response = response;
            }

            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                LastRequest = request;
                return Task.FromResult(_response);
            }
        }

        private ProductIntegrationService CreateService(out StubHandler handler, HttpStatusCode statusCode = HttpStatusCode.OK)
        {
            var response = new HttpResponseMessage(statusCode);
            handler = new StubHandler(response);
            var client = new HttpClient(handler) { BaseAddress = new Uri("http://localhost") };

            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    { "ServiceAuth:Secret", "test-secret" }
                })
                .Build();

            return new ProductIntegrationService(client, config);
        }

        [Fact]
        public async Task HideProductsAsync_AddsAuthHeader_AndPostsToCorrectUrl()
        {
            var userId = Guid.NewGuid();
            var service = CreateService(out var handler);

            await service.HideProductsAsync(userId);

            Assert.NotNull(handler.LastRequest);
            Assert.Equal(HttpMethod.Post, handler.LastRequest!.Method);
            Assert.Equal($"/internal/hide-products/{userId}", handler.LastRequest!.RequestUri!.PathAndQuery);
            Assert.True(handler.LastRequest!.Headers.Contains("X-Service-Auth"));
            Assert.Equal("test-secret", handler.LastRequest!.Headers.GetValues("X-Service-Auth").First());
        }

        [Fact]
        public async Task RestoreProductsAsync_AddsAuthHeader_AndPostsToCorrectUrl()
        {
            var userId = Guid.NewGuid();
            var service = CreateService(out var handler);

            await service.RestoreProductsAsync(userId);

            Assert.NotNull(handler.LastRequest);
            Assert.Equal(HttpMethod.Post, handler.LastRequest!.Method);
            Assert.Equal($"/internal/restore-products/{userId}", handler.LastRequest!.RequestUri!.PathAndQuery);
            Assert.True(handler.LastRequest!.Headers.Contains("X-Service-Auth"));
        }

        [Fact]
        public async Task HideProductsAsync_Throws_WhenResponseIsNotSuccess()
        {
            var userId = Guid.NewGuid();
            var service = CreateService(out var handler, HttpStatusCode.BadRequest);

            await Assert.ThrowsAsync<HttpRequestException>(() => service.HideProductsAsync(userId));
        }

        [Fact]
        public async Task RestoreProductsAsync_Throws_WhenResponseIsNotSuccess()
        {
            var userId = Guid.NewGuid();
            var service = CreateService(out var handler, HttpStatusCode.InternalServerError);

            await Assert.ThrowsAsync<HttpRequestException>(() => service.RestoreProductsAsync(userId));
        }
    }
}
