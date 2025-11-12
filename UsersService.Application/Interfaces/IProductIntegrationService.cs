namespace UsersService.Application.Interfaces
{
    public interface IProductIntegrationService
    {
        Task HideProducts(Guid userId);
        Task RestoreProducts(Guid userId);
    }
}