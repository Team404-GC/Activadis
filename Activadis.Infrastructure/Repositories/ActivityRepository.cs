using Activadis.Domain.Interfaces.Repositories;
using Activadis.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Activadis.Domain.Entities;
using Activadis.Domain.Filters;

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
				.Where(x => x.DeletedAt == null)
				.Where(x => isAdmin || x.PublishedOn != null)
				.Where(x => (isAdmin && filter.IncludePast) || x.EndDate >= DateTime.UtcNow)
				.Where(x => filter.Name == null || x.Name.Contains(filter.Name))
				.Where(x => filter.To == null || x.StartDate <= filter.To)
				.Where(x => filter.From == null || x.EndDate >= filter.From)
				.OrderBy(x => x.StartDate)
				.ToListAsync();
		public async Task<Activity?> GetDetailAsync(Guid id)
			=> await Context.Activities
				.Include(x => x.SignUps.Where(signUp => signUp.DeletedAt == null))
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
