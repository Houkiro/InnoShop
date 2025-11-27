namespace UsersService.Application.Interfaces
{
    public interface IProductIntegrationService
    {
        Task HideProductsAsync(Guid userId);
        Task RestoreProductsAsync(Guid userId);
    }
}