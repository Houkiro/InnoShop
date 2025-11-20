namespace ProductsService.Application.Interfaces
{
    public interface IBaseRepository<T> where T : class
    {
        Task<T?> GetByIdAsync(Guid id);
        IQueryable<T> GetQueryable();
        Task AddAsync(T entity);
        Task UpdateAsync(T entity);
    }
}