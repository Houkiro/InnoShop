using System.Net.Http.Json;
using UsersService.Application.Interfaces;

namespace UsersService.Application.Services
{
    public class ProductServiceClient : IProductServiceClient
    {
        private readonly HttpClient _http;

        public ProductServiceClient(HttpClient http)
        {
            _http = http;
        }

        public async Task HideProductsByUserIdAsync(Guid userId)
        {
            var response = await _http.PostAsJsonAsync(
                $"api/products/hide-by-user/{userId}", new { });
        }

        public async Task RestoreProductsByUserIdAsync(Guid userId)
        {
            var response = await _http.PostAsJsonAsync(
                $"api/products/restore-by-user/{userId}", new { });
        }
    }
}