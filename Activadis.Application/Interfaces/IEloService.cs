using Activadis.Shared.DTOs.Elo;
using Activadis.Shared.DTOs.Elo.Category;
using Activadis.Shared.DTOs.Elo.Leaderboard;
using Activadis.Shared.DTOs.Elo.Match;

namespace Activadis.Application.Interfaces
{
    public interface IEloService
    {
        Task<MatchResultDto> RecordMatchAsync(RecordMatchRequest request);

        Task<PlayerProfileDto?> GetPlayerProfileAsync(Guid userId);

        Task<List<LeaderboardEntryDto>> GetCategoryLeaderboardAsync(Guid categoryId);

        Task<List<LeaderboardEntryDto>> GetOverallLeaderboardAsync();

        Task<List<CategoryDto>> GetCategoriesAsync();

        Task<CategoryDto> CreateCategoryAsync(CreateCategoryRequest request);

        Task<CategoryDto?> UpdateCategoryAsync(UpdateCategoryRequest request);
    }
}
