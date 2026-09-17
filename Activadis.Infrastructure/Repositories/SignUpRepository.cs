using Activadis.Domain.Interfaces.Repositories;
using Activadis.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Activadis.Domain.Entities;

namespace Activadis.Infrastructure.Repositories
{
    public class SignUpRepository : Repository<SignUp>, ISignUpRepository
    {
        private readonly ApplicationDBContext Context;

        public SignUpRepository(ApplicationDBContext context)
            : base(context)
        {
            Context = context;
        }

        public async Task<SignUp?> GetByUserIdAndActivityIdIncludingDeletedAsync(Guid userId, Guid activityId)
            => await Context.SignUps.SingleOrDefaultAsync(x => x.UserId == userId && x.ActivityId == activityId);

        public async Task<bool> HasSignedUpAsync(Guid userId, Guid activityId)
            => await Context.SignUps.AnyAsync(x => x.UserId == userId && x.ActivityId == activityId && x.DeletedAt == null);
    }
}
