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

        public async Task<SignUp?> GetByEmailAndActivityIdIncludingDeletedAsync(string email, Guid activityId)
            => await Context.SignUps.SingleOrDefaultAsync(x => x.Email == email && x.ActivityId == activityId);

        public async Task<SignUp?> GetByUserIdAndActivityIdAsync(Guid userId, Guid activityId)
            => await Context.SignUps.SingleOrDefaultAsync(x => x.UserId == userId && x.ActivityId == activityId && x.DeletedAt == null);

        public async Task<SignUp?> GetByEmailAndActivityIdAsync(string email, Guid activityId)
            => await Context.SignUps.SingleOrDefaultAsync(x => x.Email == email && x.ActivityId == activityId && x.DeletedAt == null);

        public async Task<bool> HasSignedUpAsync(Guid userId, Guid activityId)
            => await Context.SignUps.AnyAsync(x => x.UserId == userId && x.ActivityId == activityId && x.DeletedAt == null);

        public async Task<string?> GetFullNameByEmailAndActivityIdAsync(string email, Guid activityId)
            => await Context.SignUps.Where(x => x.Email == email && x.ActivityId == activityId && x.DeletedAt == null)
                .Select(x => x.FullName)
                .SingleOrDefaultAsync();

        public async Task<int> CountByActivityIdAsync(Guid activityId)
            => await Context.SignUps.CountAsync(x => x.ActivityId == activityId && x.DeletedAt == null);
    }
}
