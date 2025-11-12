using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductsService.Infrastructure.Services
{
    public class ProductIntegrationService
    {
        private readonly HttpClient _http;
        private readonly IConfiguration _config;

        public ProductIntegrationService(HttpClient http, IConfiguration config)
        {
            _http = http;
            _config = config;
        }

        public async Task HideProducts(Guid userId)
        {
            var secret = _config["ServiceAuth:Secret"];
            _http.DefaultRequestHeaders.Add("X-Service-Auth", secret);

            var response = await _http.PostAsync($"/internal/hide-products/{userId}", null);
            response.EnsureSuccessStatusCode();
        }

        public async Task RestoreProducts(Guid userId)
        {
            var secret = _config["ServiceAuth:Secret"];
            _http.DefaultRequestHeaders.Add("X-Service-Auth", secret);

            var response = await _http.PostAsync($"/internal/restore-products/{userId}", null);
            response.EnsureSuccessStatusCode();
        }
    }
}
