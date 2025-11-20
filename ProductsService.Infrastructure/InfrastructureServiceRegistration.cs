using LoggingService;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ProductsService.Application.Interfaces;
using ProductsService.Infrastructure.Persistence;
using ProductsService.Infrastructure.Repositories;
using ProductsService.Infrastructure.Services;

namespace ProductsService.Infrastructure
{
    public static class InfrastructureServiceRegistration
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<ProductDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

            services.AddScoped<IProductRepository, ProductRepository>();
            services.AddScoped<IProductService, ProductService>();

            services.AddScoped<ICurrentUserService, CurrentUserService>();

            services.AddHttpContextAccessor();

            services.AddHttpClient<IProductIntegrationService, ProductIntegrationService>(client =>
            {
                client.BaseAddress = new Uri(configuration["ProductService:BaseUrl"] ?? "https://localhost:7240");
            });

            services.AddScoped<IUnitOfWork, UnitOfWork>();
            return services;
        }
        public static void ConfigureLoggerService(this IServiceCollection services) =>
            services.AddSingleton<ILoggerManager, LoggerManager>();
    }
}