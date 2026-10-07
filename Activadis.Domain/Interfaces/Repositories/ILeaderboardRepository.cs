using Activadis.Domain.Entities;

namespace Activadis.Domain.Interfaces.Repositories
{
    public interface ILeaderboardRepository : IRepository<Leaderboard>
    {
        Task<Leaderboard?> GetLatestCategoryLeaderboardAsync(Guid categoryId);

        Task<Leaderboard?> GetLatestOverallLeaderboardAsync();

        Task<Leaderboard> SaveLeaderboardAsync(Leaderboard leaderboard);

        Task CleanupOldLeaderboardsAsync(int keepCount = 1);
    }
}
