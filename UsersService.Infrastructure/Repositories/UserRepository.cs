using Microsoft.EntityFrameworkCore;
using UsersService.Application.Interfaces;
using UsersService.Domain.Entities;
using UsersService.Infrastructure.Persistence;

namespace UsersService.Infrastructure.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly UserDbContext _db;

        public UserRepository(UserDbContext db)
        {
            _db = db;
        }

        public Task<User?> GetByIdAsync(Guid id) =>
            _db.Users.FirstOrDefaultAsync(u => u.Id == id);

        public Task<User?> GetByEmailAsync(string email) =>
            _db.Users.FirstOrDefaultAsync(u => u.Email == email);

        public Task<bool> EmailExistsAsync(string email) =>
            _db.Users.AnyAsync(u => u.Email == email);

        public Task<User?> GetByConfirmationTokenAsync(string token) =>
            _db.Users.FirstOrDefaultAsync(u => u.ConfirmationToken == token);

        public Task<User?> GetByResetPasswordTokenAsync(string token) =>
            _db.Users.FirstOrDefaultAsync(u => u.PasswordResetToken == token);

        public Task AddAsync(User user)
        {
            _db.Users.Add(user);
            return Task.CompletedTask;
        }

        public Task UpdateAsync(User user)
        {
            _db.Users.Update(user);
            return Task.CompletedTask;
        }
    }
}