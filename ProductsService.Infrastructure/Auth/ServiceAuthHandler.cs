using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace ProductsService.Auth
{
    public class ServiceAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        private readonly IConfiguration _config;
        private readonly ILoggerFactory _loggerFactory;

        public ServiceAuthHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory loggerFactory,
            UrlEncoder encoder,
            ISystemClock clock,
            IConfiguration config)
            : base(options, loggerFactory, encoder, clock)
        {
            _config = config;
            _loggerFactory = loggerFactory;
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            // Логирование ошибок и успешных аутентификаций
            var logger = _loggerFactory.CreateLogger<ServiceAuthHandler>();

            // Проверка на наличие заголовка X-Service-Auth
            if (!Request.Headers.TryGetValue("X-Service-Auth", out var providedSecret))
            {
                logger.LogError("Missing X-Service-Auth header");
                return Task.FromResult(AuthenticateResult.Fail("Missing header"));
            }

            // Проверка на совпадение с конфигурированным секретом
            var expectedSecret = _config["ServiceAuth:Secret"];
            if (expectedSecret == null || providedSecret != expectedSecret)
            {
                logger.LogError("Invalid Service Secret");
                return Task.FromResult(AuthenticateResult.Fail("Invalid service secret"));
            }

            // Создание и возвращение результата аутентификации
            var claims = new[] { new Claim(ClaimTypes.Name, "InternalService") };
            var identity = new ClaimsIdentity(claims, Scheme.Name);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, Scheme.Name);

            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
    }
}
