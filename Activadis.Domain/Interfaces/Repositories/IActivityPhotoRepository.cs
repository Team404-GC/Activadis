using Activadis.Domain.Entities;

namespace Activadis.Domain.Interfaces.Repositories
{
	public interface IActivityPhotoRepository : IRepository<ActivityPhoto>
	{
		Task UpdateSortOrderAsync(Guid id, int sortOrder);
		Task<ActivityPhoto?> GetByIdAndActivityIdAsync(Guid id, Guid activityId);
		Task<ActivityPhoto?> GetFirstByActivityIdAsync(Guid activityId);

		Task<IEnumerable<Guid>> GetIdsByActivityIdAsync(Guid activityId);
		Task<IEnumerable<Guid>> GetActivityIdsWithPhotosAsync(IEnumerable<Guid> activityIds);
		Task<int> CountByActivityIdAsync(Guid activityId);
	}
}