using Activadis.Shared.DTOs;
using Activadis.Shared.DTOs.Elo;
using Activadis.Shared.DTOs.Elo.Category;
using Activadis.Shared.DTOs.Elo.Leaderboard;
using Activadis.Shared.DTOs.Elo.Match;
using Activadis.Shared.DTOs.Elo.Stats;
using Activadis.UI.Application.DTOs.Activity;

namespace Activadis.UI.Application.Interfaces
{
    public interface IEloService
    {
        Task<ApiResponse<List<LeaderboardEntryDto>>> GetOverallLeaderboardAsync();
        Task<ApiResponse<List<LeaderboardEntryDto>>> GetCategoryLeaderboardAsync(string categoryId);
        Task<ApiResponse<PlayerProfileDto>> GetPlayerProfileAsync(string userId);
        Task<ApiResponse<List<CategoryDto>>> GetCategoriesAsync();
        Task<ApiResponse<MatchResultDto>> RecordMatchAsync(RecordMatchRequest request);
        Task<ApiResponse<object>> CreateCategoryAsync(CreateCategoryRequest request);
        Task<ApiResponse<CategoryDto>> UpdateCategoryAsync(UpdateCategoryRequest request);
        Task<ApiResponse<List<PlayerRatingHistoryDto>>> GetRatingHistoryAsync(Guid categoryId, int top = 5, List<Guid>? userIds = null);
        Task<ApiResponse<EloSummaryDto>> GetSummaryAsync(Guid? categoryId = null);
        Task<ApiResponse<List<ActivityPointDto>>> GetActivityAsync(Guid? categoryId = null, int months = 12);
        Task<ApiResponse<List<RecentMatchDto>>> GetRecentMatchesAsync(Guid? categoryId = null, int take = 20);
    }
}
