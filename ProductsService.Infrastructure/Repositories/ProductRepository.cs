using Microsoft.EntityFrameworkCore;
using ProductsService.Application.Interfaces;
using ProductsService.Infrastructure.Persistence;

namespace ProductsService.Infrastructure.Repositories
{
    public class ProductRepository : IProductRepository
    {
        private readonly ProductDbContext _context;

        public ProductRepository(ProductDbContext context)
        {
            _context = context;
        }

        public async Task HideProductsByUserIdAsync(Guid userId)
        {
            var products = await _context.Products
                .Where(p => p.UserId == userId)
                .ToListAsync();

            foreach (var product in products) 
                product.IsDeleted = true;

            await _context.SaveChangesAsync();
        }

        public async Task RestoreProductsByUserIdAsync(Guid userId)
        {
            var products = await _context.Products
                .Where(p => p.UserId == userId)
                .ToListAsync();

            foreach (var product in products)
                product.IsDeleted = false;

            await _context.SaveChangesAsync();
        }
    }
}
