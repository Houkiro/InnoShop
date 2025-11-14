using System.Threading.Tasks;

namespace ProductsService.Application.Interfaces
{
    public interface IUnitOfWork
    {
        Task<int> SaveChangesAsync();
    }
}