using Activadis.Domain.Entities;

namespace Activadis.Domain.Interfaces.Repositories
{
    public interface IWaitlistRepository : IRepository<WaitlistEntry>
    {
        Task<int> GetLastPositionAsync(Guid activityId);
    }
}
