using Activadis.Domain.Entities;
using Activadis.Domain.Interfaces.Repositories;

namespace Activadis.Domain.Interfaces.Repositories
{
    public interface IRatingRepository : IRepository<Rating>
    {
        Task<IEnumerable<Rating>> GetUserRatingsAsync(Guid userId);

        Task<IEnumerable<Rating>> GetCategoryRatingsAsync(Guid categoryId);

        Task<Rating?> GetUserCategoryRatingAsync(Guid userId, Guid categoryId);

        Task<Rating> GetOrCreateRatingAsync(Guid userId, Guid categoryId);
    }
}
