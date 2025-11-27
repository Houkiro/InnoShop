using ProductsService.Application.Interfaces;
using ProductsService.Infrastructure.Persistence;

namespace ProductsService.Infrastructure
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ProductDbContext _db;

        public UnitOfWork(ProductDbContext db)
        {
            _db = db;
        }

        public Task<int> SaveChangesAsync()
        {
            return _db.SaveChangesAsync();
        }
    }
}