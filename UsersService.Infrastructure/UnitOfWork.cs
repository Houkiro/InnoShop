using UsersService.Application.Interfaces;
using UsersService.Infrastructure.Persistence;

namespace UsersService.Infrastructure
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly UserDbContext _db;

        public UnitOfWork(UserDbContext db)
        {
            _db = db;
        }

        public Task<int> SaveChangesAsync()
        {
            return _db.SaveChangesAsync();
        }
    }
}