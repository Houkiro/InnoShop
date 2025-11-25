using System.Net;
using UsersService.Application.Services;

namespace UsersService.Tests.Services
{
    public class ProductServiceClientTests
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

        private ProductServiceClient CreateService(out StubHandler handler)
        {
            var response = new HttpResponseMessage(HttpStatusCode.OK);
            handler = new StubHandler(response);
            var client = new HttpClient(handler) { BaseAddress = new Uri("http://localhost") };
            return new ProductServiceClient(client);
        }

        [Fact]
        public async Task HideProductsByUserIdAsync_SendsPostToCorrectUrl()
        {
            var userId = Guid.NewGuid();
            var service = CreateService(out var handler);

            await service.HideProductsByUserIdAsync(userId);

            Assert.NotNull(handler.LastRequest);
            Assert.Equal(HttpMethod.Post, handler.LastRequest!.Method);
            Assert.Equal($"/api/products/hide-by-user/{userId}", handler.LastRequest!.RequestUri!.PathAndQuery);
        }

        [Fact]
        public async Task RestoreProductsByUserIdAsync_SendsPostToCorrectUrl()
        {
            var userId = Guid.NewGuid();
            var service = CreateService(out var handler);

            await service.RestoreProductsByUserIdAsync(userId);

            Assert.NotNull(handler.LastRequest);
            Assert.Equal(HttpMethod.Post, handler.LastRequest!.Method);
            Assert.Equal($"/api/products/restore-by-user/{userId}", handler.LastRequest!.RequestUri!.PathAndQuery);
        }

        [Fact]
        public async Task HideProductsByUserIdAsync_SendsEmptyJsonBody()
        {
            var userId = Guid.NewGuid();
            var service = CreateService(out var handler);

            await service.HideProductsByUserIdAsync(userId);

            var body = await handler.LastRequest!.Content!.ReadAsStringAsync();
            Assert.Equal("{}", body);
        }

        [Fact]
        public async Task RestoreProductsByUserIdAsync_SendsEmptyJsonBody()
        {
            var userId = Guid.NewGuid();
            var service = CreateService(out var handler);

            await service.RestoreProductsByUserIdAsync(userId);

            var body = await handler.LastRequest!.Content!.ReadAsStringAsync();
            Assert.Equal("{}", body);
        }
    }
}