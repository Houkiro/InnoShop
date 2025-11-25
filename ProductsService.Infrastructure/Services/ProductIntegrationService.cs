using ProductsService.Application.Interfaces;

namespace ProductsService.Infrastructure.Services
{
    public class ProductIntegrationService : IProductIntegrationService
    {
        private readonly HttpClient _http;

        public ProductIntegrationService(HttpClient http)
        {
            _http = http;
        }

        public async Task HideProductsAsync(Guid userId)
        {
            var response = await _http.PostAsync($"/api/internal/products/hide-by-user/{userId}", null);
            response.EnsureSuccessStatusCode();
        }

        public async Task RestoreProductsAsync(Guid userId)
        {
            var response = await _http.PostAsync($"/api/internal/products/restore-by-user/{userId}", null);
            response.EnsureSuccessStatusCode();
        }
    }
}