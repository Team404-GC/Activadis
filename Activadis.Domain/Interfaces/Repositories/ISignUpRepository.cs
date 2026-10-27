using Activadis.Domain.Entities;

namespace Activadis.Domain.Interfaces.Repositories
{
    public interface ISignUpRepository : IRepository<SignUp>
    {
        Task<SignUp?> GetByUserIdAndActivityIdIncludingDeletedAsync(Guid userId, Guid activityId);
        Task<SignUp?> GetByEmailAndActivityIdIncludingDeletedAsync(string email, Guid activityId);
        Task<SignUp?> GetByUserIdAndActivityIdAsync(Guid userId, Guid activityId);
        Task<SignUp?> GetByEmailAndActivityIdAsync(string email, Guid activityId);
        Task<IEnumerable<SignUp>> GetUpcomingByUserIdAsync(Guid userId);
        Task<bool> HasSignedUpAsync(Guid userId, Guid activityId);
        Task<string?> GetFullNameByEmailAndActivityIdAsync(string email, Guid activityId);
        Task<int> CountByActivityIdAsync(Guid activityId);
        Task<int> GetLastPositionAsync(Guid activityId);
        Task<SignUp?> GetFirstPositionAsync(Guid activityId);
        Task<SignUp?> GetByEmailAndActivityIdFromWaitlistAsync(string email, Guid activityId);
        Task AdvanceWaitlistAsync(int positionToRemove, Guid activityId);
    }
}
