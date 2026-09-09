using Activadis.Domain.Interfaces.Repositories;
using Activadis.Infrastructure.Persistence;
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
    }
}
