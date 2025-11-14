using Microsoft.Extensions.Options;
using UsersService.Application.Interfaces;

namespace UsersService.Infrastructure.Auth
{
    public class JwtSettingsProvider : IJwtSettingsProvider
    {
        private readonly JwtSettings _settings;
        public JwtSettingsProvider(IOptions<JwtSettings> options)
        {
            _settings = options.Value;
        }

        public string Key => _settings.Key;
        public string Issuer => _settings.Issuer;
        public string Audience => _settings.Audience;
        public int ExpirationMinutes => _settings.ExpirationMinutes;
    }
}
