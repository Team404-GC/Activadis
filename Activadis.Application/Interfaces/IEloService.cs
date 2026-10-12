using Activadis.Shared.DTOs.Elo;
using Activadis.Shared.DTOs.Elo.Category;
using Activadis.Shared.DTOs.Elo.Leaderboard;
using Activadis.Shared.DTOs.Elo.Match;
using Activadis.Shared.DTOs.Elo.Stats;

namespace Activadis.Application.Interfaces
{
    public interface IEloService
    {
        Task<MatchResultDto> RecordMatchAsync(RecordMatchRequest request);

        Task<PlayerProfileDto?> GetPlayerProfileAsync(Guid userId);

        Task<List<LeaderboardEntryDto>> GetCategoryLeaderboardAsync(Guid categoryId);

        Task<List<LeaderboardEntryDto>> GetOverallLeaderboardAsync();

        Task<List<CategoryDto>> GetCategoriesAsync();

        Task CreateCategoryAsync(CreateCategoryRequest request);

        Task<CategoryDto?> UpdateCategoryAsync(UpdateCategoryRequest request);

        Task<List<PlayerRatingHistoryDto>> GetRatingHistoryAsync(Guid categoryId, int top = 5, List<Guid>? userIds = null);

        Task<EloSummaryDto> GetSummaryAsync(Guid? categoryId = null);

        Task<List<ActivityPointDto>> GetActivityAsync(Guid? categoryId = null, int months = 12);

        Task<List<RecentMatchDto>> GetRecentMatchesAsync(Guid? categoryId = null, int take = 20);
    }
}
