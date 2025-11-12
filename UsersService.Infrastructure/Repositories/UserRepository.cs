using Microsoft.EntityFrameworkCore;
using UsersService.Application.Interfaces;
using UsersService.Domain.Entities;
using UsersService.Infrastructure.Persistence;

namespace UsersService.Infrastructure.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly UserDbContext _context;
        public UserRepository(UserDbContext context)
        {
            _context = context;
        }

        public async Task AddUserAsync(User user)
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
        }

        public async Task<User> GetByEmailAsync(string email) =>
            await _context.Users.FirstOrDefaultAsync(u => u.Email == email);

        public async Task<User> GetByIdAsync(Guid id) =>
            await _context.Users.FindAsync(id);
        public async Task UpdateUserAsync(User user)
        {
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
        }
        public async Task SaveChangesAsync()
            => await _context.SaveChangesAsync();
    }
}