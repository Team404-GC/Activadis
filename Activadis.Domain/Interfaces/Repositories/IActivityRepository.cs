using Activadis.Domain.Entities;

namespace Activadis.Domain.Interfaces.Repositories
{
    public interface IActivityRepository : IRepository<Activity>
    {
        Task<IEnumerable<Activity>> GetUpcomingAsync();
        Task<Activity?> GetDetailAsync(Guid id);
        Task<IEnumerable<Activity>> GetSignedUpByUserIdAsync(Guid userId);
    }
}
