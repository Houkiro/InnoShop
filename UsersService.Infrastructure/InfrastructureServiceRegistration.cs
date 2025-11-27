using LoggingService;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using UsersService.Application.Interfaces;
using UsersService.Application.Services;
using UsersService.Infrastructure.Auth;
using UsersService.Infrastructure.Persistence;
using UsersService.Infrastructure.Repositories;
using UsersService.Infrastructure.Services;

namespace UsersService.Infrastructure
{
    public static class InfrastructureServiceRegistration
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<UserDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            services.AddScoped<IPasswordHasher, PasswordHasher>();

            services.Configure<JwtSettings>(configuration.GetSection("JwtSettings"));
            services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
            services.AddSingleton<IJwtSettingsProvider, JwtSettingsProvider>();

            var jwtSettings = configuration.GetSection("JwtSettings").Get<JwtSettings>();
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSettings.Issuer,
                    ValidAudience = jwtSettings.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key))
                };
            });

            services.Configure<EmailSettings>(configuration.GetSection("EmailSettings"));
            services.AddSingleton(sp => sp.GetRequiredService<IOptions<EmailSettings>>().Value);
            services.AddScoped<IEmailService, EmailService>();

            services.AddHttpClient<IProductServiceClient, ProductServiceClient>(client =>
            {
                client.BaseAddress = new Uri(configuration["ProductService:BaseUrl"]);
            });

            services.AddHttpClient<IProductIntegrationService, ProductIntegrationService>(client =>
            {
                client.BaseAddress = new Uri(configuration["Services:Products:BaseUrl"] ??
                                              throw new Exception("Services:Products:BaseUrl not configured"));
            });
            services.AddHttpContextAccessor();
            services.AddScoped<ICurrentUserService, CurrentUserService>();
            services.AddScoped<ISmtpClientWrapper, SmtpClientWrapper>();
            services.AddScoped<IEmailService, EmailService>();
            services.AddSingleton<ILoggingService, NLogService>();
            return services;
        }
    }
}