using Microsoft.EntityFrameworkCore;
using ProductsService.Application.Interfaces;
using ProductsService.Domain.Entities;
using ProductsService.Infrastructure.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
        }

        public async Task<Product?> GetByIdAsync(Guid id)
        {
            return await _context.Products.FindAsync(id);
        }

        public IQueryable<Product> GetQueryable()
        {
            return _context.Products.AsQueryable();
        }

        public Task UpdateAsync(Product product)
        {
            _context.Products.Update(product);
            return Task.CompletedTask;
        }

        public async Task<List<Product>> GetByUserIdAsync(Guid userId)
        {
            return await _context.Products
                .IgnoreQueryFilters()
                .Where(p => p.UserId == userId)
                .ToListAsync();
        }
    }
}