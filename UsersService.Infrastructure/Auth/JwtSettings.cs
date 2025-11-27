namespace UsersService.Infrastructure.Auth
{
    public class JwtSettings
    {
        public string Issuer { get; set; } = default!;
        public string Audience { get; set; } = default!;
        public string Key { get; set; } = default!;
        public int ExpirationMinutes { get; set; } = 60;
    }
}