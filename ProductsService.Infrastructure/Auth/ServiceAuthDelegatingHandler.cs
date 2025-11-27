using Microsoft.Extensions.Configuration;

namespace ProductsService.Infrastructure.Auth
{
    public class ServiceAuthDelegatingHandler : DelegatingHandler
    {
        private readonly IConfiguration _config;

        public ServiceAuthDelegatingHandler(IConfiguration config)
        {
            _config = config;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var secret = _config["ServiceAuth:Secret"];
            if (!string.IsNullOrEmpty(secret))
            {
                request.Headers.Add("X-Service-Auth", secret);
            }

            return await base.SendAsync(request, cancellationToken);
        }
    }
}