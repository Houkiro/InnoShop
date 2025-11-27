using ProductsService.Application.Commands.CreateProduct;
using ProductsService.Application.Commands.UpdateProduct;

namespace ProductsService.Application.Interfaces
{
    public interface IProductService
    {
        Task<Guid> CreateProductAsync(CreateProductCommand command, Guid userId);
        Task UpdateProductAsync(UpdateProductCommand command, Guid userId);
        Task DeleteProductAsync(Guid productId, Guid userId);
    }
}