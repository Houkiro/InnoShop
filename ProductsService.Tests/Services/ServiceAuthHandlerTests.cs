using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ProductsService.Auth;
using System.Text.Encodings.Web;

namespace ProductsService.Tests.Services
{
    public class FakeOptionsMonitor<T> : IOptionsMonitor<T> where T : class, new()
    {
        private readonly T _current;
        public FakeOptionsMonitor(T current) { _current = current; }
        public T CurrentValue => _current;
        public T Get(string name) => _current;
        public IDisposable? OnChange(Action<T, string> listener) => null!;
    }

    public class ServiceAuthHandlerTests
    {
        private ServiceAuthHandler CreateHandler(HttpContext context, string? secretInConfig)
        {
            var optionsMonitor = new FakeOptionsMonitor<AuthenticationSchemeOptions>(new AuthenticationSchemeOptions());
            var loggerFactory = LoggerFactory.Create(builder => builder.AddDebug());
            var encoder = UrlEncoder.Default;
            var clock = new SystemClock();

            var inMemory = new Dictionary<string, string>();
            if (secretInConfig != null) inMemory["ServiceAuth:Secret"] = secretInConfig;

            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemory)
                .Build();

            var handler = new ServiceAuthHandler(optionsMonitor, loggerFactory, encoder, clock, config);
            handler.InitializeAsync(new AuthenticationScheme("TestScheme", null, typeof(ServiceAuthHandler)), context).GetAwaiter().GetResult();
            return handler;
        }

        [Fact]
        public async Task HandleAuthenticateAsync_Fails_WhenHeaderMissing()
        {
            var context = new DefaultHttpContext();
            var handler = CreateHandler(context, "expected-secret");

            var result = await handler.AuthenticateAsync();

            Assert.False(result.Succeeded);
            Assert.Equal("Missing header", result.Failure?.Message);
        }

        [Fact]
        public async Task HandleAuthenticateAsync_Fails_WhenSecretInvalid()
        {
            var context = new DefaultHttpContext();
            context.Request.Headers["X-Service-Auth"] = "wrong-secret";
            var handler = CreateHandler(context, "expected-secret");

            var result = await handler.AuthenticateAsync();

            Assert.False(result.Succeeded);
            Assert.Equal("Invalid service secret", result.Failure?.Message);
        }

        [Fact]
        public async Task HandleAuthenticateAsync_Succeeds_WhenSecretValid()
        {
            var context = new DefaultHttpContext();
            context.Request.Headers["X-Service-Auth"] = "expected-secret";
            var handler = CreateHandler(context, "expected-secret");

            var result = await handler.AuthenticateAsync();

            Assert.True(result.Succeeded);
            Assert.NotNull(result.Principal);
            Assert.Equal("InternalService", result.Principal!.Identity!.Name);
        }
    }
}