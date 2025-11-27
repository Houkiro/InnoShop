namespace UsersService.Application.Interfaces
{
    public interface IJwtSettingsProvider
    {
        string Key { get; }
        string Issuer { get; }
        string Audience { get; }
        int ExpirationMinutes { get; }
    }
}