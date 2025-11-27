using UsersService.Domain.Entities;

namespace UsersService.Application.Interfaces
{
    public interface IJwtTokenGenerator
    {
        string GenerateToken(User user);
    }
}