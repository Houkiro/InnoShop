using UsersService.Application.Interfaces;

namespace UsersService.Infrastructure.Auth
{
    public class TokenGeneratorService : ITokenGeneratorService
    {
        public string GenerateToken()
        {
            return Guid.NewGuid().ToString();
        }

        public DateTime GetExpiration(int hours = 1)
        {
            return DateTime.UtcNow.AddHours(hours);
        }
    }
}