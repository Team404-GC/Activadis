using Activadis.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Activadis.Domain.Interfaces;

namespace Activadis.Infrastructure.Repositories
{
    public class Repository<TEntity> : IRepository<TEntity> where TEntity : class, IEntity
    {
        private readonly ApplicationDBContext Context;
        private readonly DbSet<TEntity> Set;

        public Repository(ApplicationDBContext context)
        {
            Context = context;
            Set = Context.Set<TEntity>();
        }

        public async Task<IEnumerable<TEntity>> GetAllAsync()
            => await Set
                .Where(x => x.DeletedAt == null)
                .ToListAsync();

        public async Task<TEntity?> GetByIdAsync(Guid id)
            => await Set.SingleOrDefaultAsync(x => x.DeletedAt == null);

        public async Task<TEntity> AddAsync(TEntity entity)
        {
            entity.CreatedAt = DateTime.UtcNow;
            Set.Add(entity);
            await Context.SaveChangesAsync();
            return entity;
        }

        public async Task UpdateAsync(TEntity entity)
        {
            entity.UpdatedAt = DateTime.UtcNow;
            Set.Update(entity);
            await Context.SaveChangesAsync();
        }

        public async Task DeleteAsync(TEntity entity)
        {
            entity.DeletedAt = DateTime.UtcNow;
            Set.Update(entity);
            await Context.SaveChangesAsync();
        }
    }
}
