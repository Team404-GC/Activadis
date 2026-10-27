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
            => await Context.SignUps.CountAsync(x => x.ActivityId == activityId && x.WaitlistPosition == null && x.DeletedAt == null);

        public async Task<int> GetLastPositionAsync(Guid activityId)
            => await Context.SignUps
                .Where(x => x.ActivityId == activityId && x.DeletedAt == null)
                .OrderByDescending(x => x.WaitlistPosition)
                .Select(x => x.WaitlistPosition)
                .FirstOrDefaultAsync() ?? 0;

        public async Task<SignUp?> GetFirstPositionAsync(Guid activityId)
            => await Context.SignUps
                .Where(x => x.ActivityId == activityId && x.WaitlistPosition != null && x.DeletedAt == null)
                .OrderBy(x => x.WaitlistPosition)
                .FirstOrDefaultAsync();

        public async Task<SignUp?> GetByEmailAndActivityIdFromWaitlistAsync(string email, Guid activityId)
            => await Context.SignUps.SingleOrDefaultAsync(x => x.Email == email && x.ActivityId == activityId && x.WaitlistPosition != null && x.DeletedAt == null);

        public async Task AdvanceWaitlistAsync(int position, Guid activityId)
            => await Context.SignUps
                .Where(x => x.WaitlistPosition > position && x.ActivityId == activityId && x.DeletedAt == null)
                .ExecuteUpdateAsync(property => property.SetProperty(x => x.WaitlistPosition, x => x.WaitlistPosition - 1));

        public async Task<IEnumerable<SignUp>> GetParticipantsByActivityIdAsync(Guid activityId)
            => await Context.SignUps
                .Where(x => x.ActivityId == activityId && x.WaitlistPosition == null && x.DeletedAt == null)
                .Include(x => x.User)
                .ToListAsync();
    }
}
