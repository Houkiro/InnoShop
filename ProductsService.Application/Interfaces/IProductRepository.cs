namespace ProductsService.Application.Interfaces
{
    public interface IProductRepository
    {
        Task HideProductsByUserIdAsync(Guid userId);
        Task RestoreProductsByUserIdAsync(Guid userId);
    }
}