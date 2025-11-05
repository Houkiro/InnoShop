using Microsoft.EntityFrameworkCore;
using UsersService.Domain.Entities;

namespace UsersService.Infrastructure.Persistence
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }
        public DbSet<User> Users => Set<User>();
    }
}