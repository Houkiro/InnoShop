using Microsoft.EntityFrameworkCore;
using ProductsService.Application.Interfaces;
using ProductsService.Domain.Entities;
using ProductsService.Infrastructure.Persistence;

namespace ProductsService.Infrastructure.Repositories
{
    public class ProductRepository : BaseRepository<Product>, IProductRepository
    {
        public ProductRepository(ProductDbContext context) : base(context)
        {
        }

        public async Task<List<Product>> GetByUserIdAsync(Guid userId)
        {
            return await _context.Set<Product>()
                                 .IgnoreQueryFilters() 
                                 .Where(p => p.UserId == userId)
                                 .ToListAsync();
        }
    }
}