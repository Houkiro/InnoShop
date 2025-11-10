using Microsoft.EntityFrameworkCore;
using ProductsService.Application.Interfaces;
using ProductsService.Domain.Entities;
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


        public async Task AddAsync(Product product)
        {
            await _context.Products.AddAsync(product);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Product product)
        {
            _context.Products.Update(product);
            await _context.SaveChangesAsync();
        }

        public async Task<Product?> GetByIdAsync(Guid id)
        {
            return await _context.Products
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public IQueryable<Product> GetQueryable()
        {
            return _context.Products.AsQueryable();
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
