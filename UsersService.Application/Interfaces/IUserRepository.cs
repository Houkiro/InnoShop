using UsersService.Domain.Entities;

namespace UsersService.Application.Interfaces
{
    public interface IUserRepository
    {
        Task<User?> GetByIdAsync(Guid id);
        Task<User?> GetByEmailAsync(string email);
        Task<bool> EmailExistsAsync(string email);

        Task<User?> GetByConfirmationTokenAsync(string token);
        Task<User?> GetByResetPasswordTokenAsync(string token);

        Task AddAsync(User user);
        Task UpdateAsync(User user);
    }
}