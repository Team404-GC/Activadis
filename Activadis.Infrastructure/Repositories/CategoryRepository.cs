using Activadis.Domain.Entities;
using Activadis.Domain.Interfaces.Repositories;
using Activadis.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Activadis.Infrastructure.Repositories
{
    public class CategoryRepository : Repository<Category>, ICategoryRepository
    {
        private readonly ApplicationDBContext _context;

        public CategoryRepository(ApplicationDBContext context)
            : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Category>> GetActiveCategoriesAsync()
            => await _context.Categories
                .Where(c => c.IsActive && c.DeletedAt == null)
                .OrderBy(c => c.DisplayOrder)
                .ToListAsync();

        public async Task<Category?> GetByNameAsync(string name)
            => await _context.Categories
                .FirstOrDefaultAsync(c => c.Name == name && c.DeletedAt == null);
    }
}
