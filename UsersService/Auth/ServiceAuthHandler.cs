using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace UsersService.Auth
{
    public class ServiceAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        private readonly IConfiguration _config;

        public ServiceAuthHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            ISystemClock clock,
            IConfiguration config)
            : base(options, logger, encoder, clock)
        {
            _config = config;
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (!Request.Headers.TryGetValue("X-Service-Auth", out var providedSecret))
                return Task.FromResult(AuthenticateResult.Fail("Missing header"));

            var expectedSecret = _config["ServiceAuth:Secret"];
            if (expectedSecret == null || providedSecret != expectedSecret)
                return Task.FromResult(AuthenticateResult.Fail("Invalid service secret"));

            var claims = new[] { new Claim(ClaimTypes.Name, "InternalService") };
            var identity = new ClaimsIdentity(claims, Scheme.Name);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, Scheme.Name);

            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
    }
}