namespace ProductsService.Application.Interfaces
{
    public interface IProductIntegrationService
    {
        Task HideProductsAsync(Guid userId);
        Task RestoreProductsAsync(Guid userId);
    }
}
