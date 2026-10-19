using Activadis.Domain.Interfaces.Repositories;
using Activadis.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Activadis.Domain.Entities;

namespace Activadis.Infrastructure.Repositories
{
    public class WaitlistRepository : Repository<WaitlistEntry>, IWaitlistRepository
    {
        private readonly ApplicationDBContext Context;

        public WaitlistRepository(ApplicationDBContext context)
            : base(context)
        {
            Context = context;
        }

        public async Task<int> GetLastPositionAsync(Guid activityId)
            => await Context.Waitlist
                .Where(we => we.ActivityId == activityId)
                .OrderByDescending(we => we.Position)
                .Select(we => we.Position)
                .SingleOrDefaultAsync();
    }
}
