using ProductsService.Domain.Entities;

namespace ProductsService.Application.Interfaces
{
    public interface IProductRepository
    {
        Task AddAsync(Product product);           
        Task<Product?> GetByIdAsync(Guid id);    
        IQueryable<Product> GetQueryable();             
        Task UpdateAsync(Product product);
        Task<List<Product>> GetByUserIdAsync(Guid userId);
    }
}