using Activadis.Domain.Interfaces.Repositories;
using Activadis.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Activadis.Domain.Entities;
using Activadis.Domain.Filters;
using Activadis.Infrastructure.Extensions;

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

		public async Task<IEnumerable<Activity>> GetListAsync(bool isAdmin, ActivityFilter filter)
			=> await Context.Activities
				.ApplyFilter(isAdmin, filter)
				.OrderBy(x => x.StartDate)
				.ToListAsync();

		public async Task<Activity?> GetDetailAsync(string identifier)
			=> await Context.Activities
				.WithIdentifier(identifier)
				.Where(x => x.DeletedAt == null)
				.Include(x => x.SignUps.Where(signUp => signUp.WaitlistPosition == null && signUp.DeletedAt == null))
				.Include(x => x.Categories)
				.Include(x => x.Matches)
					.ThenInclude(match => match.Category)
				.Include(x => x.Matches)
					.ThenInclude(match => match.Participants)
						.ThenInclude(participant => participant.User)
				.AsSplitQuery()
				.SingleOrDefaultAsync();

		public async Task<Activity?> GetWithCategoriesAndMatchesAsync(Guid id)
			=> await Context.Activities
				.Include(x => x.Categories)
				.Include(x => x.Matches)
					.ThenInclude(match => match.Category)
				.AsSplitQuery()
				.SingleOrDefaultAsync(x => x.Id == id && x.DeletedAt == null);

		public async Task<IEnumerable<Activity>> GetSignedUpByUserIdAsync(Guid userId)
			=> await Context.Activities
				.Where(x => x.DeletedAt == null
					&& x.EndDate >= DateTime.UtcNow
					&& x.SignUps.Any(signUp => signUp.UserId == userId && signUp.DeletedAt == null))
				.OrderBy(x => x.StartDate)
				.ToListAsync();
	}
}
