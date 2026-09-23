using Activadis.Shared.DTOs;
using Activadis.Shared.DTOs.Elo;
using Activadis.Shared.DTOs.Elo.Category;
using Activadis.Shared.DTOs.Elo.Leaderboard;
using Activadis.Shared.DTOs.Elo.Match;
using Activadis.UI.Application.DTOs.Activity;

namespace Activadis.UI.Application.Interfaces
{
    public interface IEloService
    {
        Task<ApiResponse<List<LeaderboardEntryDto>>> GetOverallLeaderboardAsync();
        Task<ApiResponse<List<LeaderboardEntryDto>>> GetCategoryLeaderboardAsync(Guid categoryId);
        Task<ApiResponse<PlayerProfileDto>> GetPlayerProfileAsync(Guid userId);
        Task<ApiResponse<List<CategoryDto>>> GetCategoriesAsync();
        Task<ApiResponse<MatchResultDto>> RecordMatchAsync(RecordMatchRequest request);
        Task<ApiResponse<object>> CreateCategoryAsync(CreateCategoryRequest request);
        Task<ApiResponse<CategoryDto>> UpdateCategoryAsync(UpdateCategoryRequest request);
    }
}
