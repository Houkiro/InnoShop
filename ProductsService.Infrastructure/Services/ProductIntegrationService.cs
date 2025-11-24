using Microsoft.Extensions.Configuration;
using ProductsService.Application.Interfaces;
using System.Net.Http.Headers;

namespace ProductsService.Infrastructure.Services
{
    public class ProductIntegrationService : IProductIntegrationService
    {
        private readonly HttpClient _http;
        private readonly IConfiguration _config;

        public ProductIntegrationService(HttpClient http, IConfiguration config)
        {
            _http = http;
            _config = config;
        }

        public async Task HideProductsAsync(Guid userId)
        {
            // Получаем секрет из конфигурации
            var secret = _config["ServiceAuth:Secret"] ?? throw new InvalidOperationException("ServiceAuth:Secret not configured");

            // Подготовка HTTP-запроса
            var req = new HttpRequestMessage(HttpMethod.Post, $"/api/internal/products/hide-by-user/{userId}");

            // Добавляем правильный заголовок для сервиса
            req.Headers.Add("X-Service-Auth", secret);

            // Отправка запроса
            var response = await _http.SendAsync(req);
            response.EnsureSuccessStatusCode();
        }

        public async Task RestoreProductsAsync(Guid userId)
        {
            // Получаем секрет из конфигурации
            var secret = _config["ServiceAuth:Secret"] ?? throw new InvalidOperationException("ServiceAuth:Secret not configured");

            // Подготовка HTTP-запроса
            var req = new HttpRequestMessage(HttpMethod.Post, $"/api/internal/products/restore-by-user/{userId}");

            // Добавляем правильный заголовок для сервиса
            req.Headers.Add("X-Service-Auth", secret);

            // Отправка запроса
            var response = await _http.SendAsync(req);
            response.EnsureSuccessStatusCode();
        }
    }
}