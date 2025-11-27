using Microsoft.Extensions.Configuration;
using ProductsService.Infrastructure.Auth;
using System.Net;

namespace ProductsService.Tests.Services
{
    public class ServiceAuthDelegatingHandlerTests
    {
        private class DummyHandler : HttpMessageHandler
        {
            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
            }
        }

        [Fact]
        public async Task SendAsync_AddsHeader_WhenSecretIsConfigured()
        {
            var inMemorySettings = new Dictionary<string, string>
            {
                { "ServiceAuth:Secret", "test-secret" }
            };
            IConfiguration config = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();

            var handler = new ServiceAuthDelegatingHandler(config)
            {
                InnerHandler = new DummyHandler()
            };

            var invoker = new HttpMessageInvoker(handler);
            var request = new HttpRequestMessage(HttpMethod.Get, "http://localhost");

            var response = await invoker.SendAsync(request, CancellationToken.None);

            Assert.True(request.Headers.Contains("X-Service-Auth"));
            Assert.Equal("test-secret", request.Headers.GetValues("X-Service-Auth").First());
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task SendAsync_DoesNotAddHeader_WhenSecretIsMissing()
        {
            IConfiguration config = new ConfigurationBuilder().Build();

            var handler = new ServiceAuthDelegatingHandler(config)
            {
                InnerHandler = new DummyHandler()
            };

            var invoker = new HttpMessageInvoker(handler);
            var request = new HttpRequestMessage(HttpMethod.Get, "http://localhost");

            var response = await invoker.SendAsync(request, CancellationToken.None);

            Assert.False(request.Headers.Contains("X-Service-Auth"));
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
    }
}