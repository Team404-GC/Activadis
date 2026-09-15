using Activadis.Domain.Entities;
using Activadis.Domain.Interfaces.Repositories;

namespace Activadis.Domain.Interfaces.Repositories
{
    public interface ICategoryRepository : IRepository<Category>
    {
        Task<IEnumerable<Category>> GetActiveCategoriesAsync();

        Task<Category?> GetByNameAsync(string name);
    }
}
