using LoggingService;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using NLog;
using System.Text;
using UsersService.Application;
using UsersService.Application.Services;
using UsersService.Auth;
using UsersService.Infrastructure;
using UsersService.Infrastructure.Auth;

var builder = WebApplication.CreateBuilder(args);

// Настройка NLog
ConfigureLogging(builder.Services);

// Регистрация DI сервисов
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// Настройка аутентификации
ConfigureAuthentication(builder.Services, builder.Configuration);

// Настройка Swagger
ConfigureSwagger(builder.Services);

// Настройка контроллеров
builder.Services.AddControllers();

// Конфигурация HTTP клиента для общения с ProductService
builder.Services.AddHttpClient<ProductIntegrationService>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ProductService:BaseUrl"]);
});

var app = builder.Build();

// Использование Swagger в режиме разработки
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Включение аутентификации и авторизации
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

// Вынесенные методы для улучшения структуры
static void ConfigureLogging(IServiceCollection services)
{
    LogManager.Setup().LoadConfigurationFromFile("nlog.config");
    services.ConfigureLoggerService();
}

static void ConfigureAuthentication(IServiceCollection services, IConfiguration configuration)
{
    var jwtSettings = configuration.GetSection("JwtSettings").Get<UsersService.Infrastructure.Auth.JwtSettings>();

    services.AddAuthentication()
        .AddScheme<AuthenticationSchemeOptions, ServiceAuthHandler>("Service", options => { })
        .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = false,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtSettings.Issuer,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key))
            };
        });
}

static void ConfigureSwagger(IServiceCollection services)
{
    services.AddEndpointsApiExplorer();
    services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo
        {
            Title = "InnoShop.UsersService API",
            Version = "v1"
        });

        // Добавление конфигурации для авторизации с JWT
        c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = SecuritySchemeType.ApiKey,
            Scheme = "Bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description = "Введите JWT токен в формате: Bearer {your token}"
        });

        c.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                Array.Empty<string>()
            }
        });
    });
}
