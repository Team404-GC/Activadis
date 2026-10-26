using Activadis.Domain.Entities;

namespace Activadis.Domain.Interfaces.Repositories
{
    public interface ICategoryRepository : IRepository<Category>
    {
        Task<IEnumerable<Category>> GetActiveCategoriesAsync();
        Task<Category?> GetByNameAsync(string name);
        Task<IEnumerable<Category>> GetByIdsAsync(IEnumerable<Guid> ids);
    }
}
