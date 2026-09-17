using Activadis.Domain.Entities;

namespace Activadis.Domain.Interfaces.Repositories
{
    public interface ISignUpRepository : IRepository<SignUp>
    {
        Task<SignUp?> GetByUserIdAndActivityIdIncludingDeletedAsync(Guid userId, Guid activityId);
    }
}
