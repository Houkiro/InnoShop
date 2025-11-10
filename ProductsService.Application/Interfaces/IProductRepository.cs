using ProductsService.Domain.Entities;

namespace ProductsService.Application.Interfaces
{
    public interface IProductRepository
    {
        Task AddAsync(Product product);
        Task UpdateAsync(Product product);
        Task<Product?> GetByIdAsync(Guid id);
        IQueryable<Product> GetQueryable();
        Task HideProductsByUserIdAsync(Guid userId);
        Task RestoreProductsByUserIdAsync(Guid userId);
    }
}