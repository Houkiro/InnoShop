using Microsoft.Extensions.Configuration;
using UsersService.Application.Interfaces;

namespace UsersService.Application.Services
{
    public class ProductIntegrationService : IProductIntegrationService
    {
        private readonly HttpClient _http;
        private readonly string _secret;

        public ProductIntegrationService(HttpClient http, IConfiguration config)
        {
            _http = http;
            _secret = config["ServiceAuth:Secret"]
                ?? throw new Exception("ServiceAuth:Secret not configured");
        }

        private void AddAuthHeader()
        {
            if (!_http.DefaultRequestHeaders.Contains("X-Service-Auth"))
                _http.DefaultRequestHeaders.Add("X-Service-Auth", _secret);
        }

        public async Task HideProductsAsync(Guid userId)
        {
            AddAuthHeader();
            var response = await _http.PostAsync($"internal/hide-products/{userId}", null);
            response.EnsureSuccessStatusCode();
        }

        public async Task RestoreProductsAsync(Guid userId)
        {
            AddAuthHeader();
            var response = await _http.PostAsync($"internal/restore-products/{userId}", null);
            response.EnsureSuccessStatusCode();
        }
    }
}