namespace UsersService.Application.Interfaces
{
    public interface IProductServiceClient
    {
        Task HideProductsByUserIdAsync(Guid userId);
        Task RestoreProductsByUserIdAsync(Guid userId);
    }
}