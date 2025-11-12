using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace ProductsService.Auth
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
            if (!Request.Headers.ContainsKey("X-Service-Auth"))
                return Task.FromResult(AuthenticateResult.Fail("Missing service auth header"));

            var provided = Request.Headers["X-Service-Auth"].ToString();
            var expected = _config["ServiceAuth:Secret"];

            if (string.IsNullOrEmpty(expected) || provided != expected)
                return Task.FromResult(AuthenticateResult.Fail("Invalid service auth token"));

            var claims = new[] { new Claim(ClaimTypes.Name, "InternalService") };
            var identity = new ClaimsIdentity(claims, Scheme.Name);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, Scheme.Name);

            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
    }
}
