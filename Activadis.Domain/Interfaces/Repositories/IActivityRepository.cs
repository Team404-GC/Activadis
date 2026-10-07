using Activadis.Domain.Entities;
using Activadis.Domain.Filters;

namespace Activadis.Domain.Interfaces.Repositories
{
	public interface IActivityRepository : IRepository<Activity>
	{
		Task<IEnumerable<Activity>> GetListAsync(bool isAdmin, ActivityFilter filter);
		Task<Activity?> GetDetailAsync(Guid id);
		Task<IEnumerable<Activity>> GetSignedUpByUserIdAsync(Guid userId);
	}
}
