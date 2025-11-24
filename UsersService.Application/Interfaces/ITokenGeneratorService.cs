namespace UsersService.Application.Interfaces
{
    public interface ITokenGeneratorService
    {
        string GenerateToken();
        DateTime GetExpiration(int hours = 1);
    }
}