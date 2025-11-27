using ProductsService.Infrastructure.Services;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace ProductsService.Tests.Infrastructure.Services
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

        [Fact]
        public async Task HideProductsAsync_SendsPostToCorrectUrl_AndSucceeds()
        {
            var userId = Guid.NewGuid();
            var response = new HttpResponseMessage(HttpStatusCode.OK);
            var handler = new StubHandler(response);
            var client = new HttpClient(handler) { BaseAddress = new Uri("http://localhost") };
            var service = new ProductIntegrationService(client);

            await service.HideProductsAsync(userId);

            Assert.NotNull(handler.LastRequest);
            Assert.Equal(HttpMethod.Post, handler.LastRequest!.Method);
            Assert.Equal($"/api/internal/products/hide-by-user/{userId}", handler.LastRequest!.RequestUri!.PathAndQuery);
        }

        [Fact]
        public async Task RestoreProductsAsync_SendsPostToCorrectUrl_AndSucceeds()
        {
            var userId = Guid.NewGuid();
            var response = new HttpResponseMessage(HttpStatusCode.OK);
            var handler = new StubHandler(response);
            var client = new HttpClient(handler) { BaseAddress = new Uri("http://localhost") };
            var service = new ProductIntegrationService(client);

            await service.RestoreProductsAsync(userId);

            Assert.NotNull(handler.LastRequest);
            Assert.Equal(HttpMethod.Post, handler.LastRequest!.Method);
            Assert.Equal($"/api/internal/products/restore-by-user/{userId}", handler.LastRequest!.RequestUri!.PathAndQuery);
        }

        [Fact]
        public async Task HideProductsAsync_Throws_WhenResponseIsNotSuccess()
        {
            var userId = Guid.NewGuid();
            var response = new HttpResponseMessage(HttpStatusCode.BadRequest);
            var handler = new StubHandler(response);
            var client = new HttpClient(handler) { BaseAddress = new Uri("http://localhost") };
            var service = new ProductIntegrationService(client);

            await Assert.ThrowsAsync<HttpRequestException>(() => service.HideProductsAsync(userId));
        }

        [Fact]
        public async Task RestoreProductsAsync_Throws_WhenResponseIsNotSuccess()
        {
            var userId = Guid.NewGuid();
            var response = new HttpResponseMessage(HttpStatusCode.InternalServerError);
            var handler = new StubHandler(response);
            var client = new HttpClient(handler) { BaseAddress = new Uri("http://localhost") };
            var service = new ProductIntegrationService(client);

            await Assert.ThrowsAsync<HttpRequestException>(() => service.RestoreProductsAsync(userId));
        }
    }
}
