using Activadis.Domain.Interfaces.Repositories;
using Activadis.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Activadis.Domain.Entities;

namespace Activadis.Infrastructure.Repositories
{
	public class ActivityPhotoRepository : Repository<ActivityPhoto>, IActivityPhotoRepository
	{
		private readonly ApplicationDBContext Context;

		public ActivityPhotoRepository(ApplicationDBContext context) : base(context)
		{
			Context = context;
		}

		public async Task UpdateSortOrderAsync(Guid id, int sortOrder)
			=> await Context.ActivityPhotos
			.Where(x => x.Id == id)
			.ExecuteUpdateAsync(property => property.SetProperty(x => x.SortOrder, sortOrder));

		public async Task<ActivityPhoto?> GetByIdAndActivityIdAsync(Guid id, Guid activityId)
			=> await Context.ActivityPhotos.SingleOrDefaultAsync(x => x.Id == id && x.ActivityId == activityId && x.DeletedAt == null);

		public async Task<ActivityPhoto?> GetFirstByActivityIdAsync(Guid activityId)
			=> await Context.ActivityPhotos
				.Where(x => x.ActivityId == activityId && x.DeletedAt == null)
				.OrderBy(x => x.SortOrder)
				.FirstOrDefaultAsync();

		public async Task<IEnumerable<Guid>> GetIdsByActivityIdAsync(Guid activityId)
			=> await Context.ActivityPhotos
				.Where(x => x.ActivityId == activityId && x.DeletedAt == null)
				.OrderBy(x => x.SortOrder)
				.Select(x => x.Id)
				.ToListAsync();

		public async Task<IEnumerable<Guid>> GetActivityIdsWithPhotosAsync(IEnumerable<Guid> activityIds)
			=> await Context.ActivityPhotos
				.Where(x => activityIds.Contains(x.ActivityId) && x.DeletedAt == null)
				.Select(x => x.ActivityId)
				.Distinct()
				.ToListAsync();

		public async Task<int> CountByActivityIdAsync(Guid activityId)
			=> await Context.ActivityPhotos.CountAsync(x => x.ActivityId == activityId && x.DeletedAt == null);
	}
}
