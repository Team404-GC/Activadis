using Activadis.Domain.Interfaces.Repositories;
using Activadis.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Activadis.Domain.Entities;

namespace Activadis.Infrastructure.Repositories
{
    public class ActivityRepository : Repository<Activity>, IActivityRepository
    {
        private readonly ApplicationDBContext Context;

        public ActivityRepository(ApplicationDBContext context)
            : base(context)
        {
            Context = context;
        }

        public async Task<IEnumerable<Activity>> GetUpcomingAsync()
            => await Context.Activities
                .Where(x => x.DeletedAt == null && x.EndDate >= DateTime.UtcNow)
                .OrderBy(x => x.StartDate)
                .ToListAsync();

        public async Task<Activity?> GetDetailAsync(Guid id)
            => await Context.Activities
                .Include(x => x.SignUps.Where(signUp => signUp.DeletedAt == null))
                .SingleOrDefaultAsync(x => x.Id == id && x.DeletedAt == null);
    }
}
