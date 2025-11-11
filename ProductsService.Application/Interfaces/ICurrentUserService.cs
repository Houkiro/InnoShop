namespace ProductsService.Application.Interfaces
{
    public interface ICurrentUserService
    {
        Guid UserId { get; }
    }
}