using Activadis.Shared.DTOs;
using Activadis.Shared.DTOs.Elo;
using Activadis.Shared.DTOs.Elo.Category;
using Activadis.Shared.DTOs.Elo.Leaderboard;
using Activadis.Shared.DTOs.Elo.Match;
using Activadis.UI.Application.DTOs.Activity;
using Activadis.UI.Application.Interfaces;

namespace Activadis.UI.Application.Services
{
    public class EloService : IEloService
    {
        private readonly IHttpService HttpService;

        public EloService(IHttpService httpService)
        {
            HttpService = httpService;
        }

        public async Task<ApiResponse<object>> CreateCategoryAsync(CreateCategoryRequest request)
            => await HttpService.PostAsync<object, CreateCategoryRequest>("/Elo/categories", request);

        public async Task<ApiResponse<List<CategoryDto>>> GetCategoriesAsync()
            => await HttpService.GetAsync<List<CategoryDto>>("/Elo/categories");

        public async Task<ApiResponse<List<LeaderboardEntryDto>>> GetCategoryLeaderboardAsync(Guid categoryId)
            => await HttpService.GetAsync<List<LeaderboardEntryDto>>($"/Elo/leaderboard/category/{categoryId}");

        public async Task<ApiResponse<List<LeaderboardEntryDto>>> GetOverallLeaderboardAsync()
            => await HttpService.GetAsync<List<LeaderboardEntryDto>>("/Elo/leaderboard/overall");

        public async Task<ApiResponse<PlayerProfileDto>> GetPlayerProfileAsync(Guid userId)
            => await HttpService.GetAsync<PlayerProfileDto>($"/Elo/player/{userId}");

        public async Task<ApiResponse<MatchResultDto>> RecordMatchAsync(RecordMatchRequest request)
            => await HttpService.PostAsync<MatchResultDto, RecordMatchRequest>("/Elo/match/record", request);

        public async Task<ApiResponse<CategoryDto>> UpdateCategoryAsync(UpdateCategoryRequest request)
            => await HttpService.PutAsync<CategoryDto, UpdateCategoryRequest>($"/Elo/player", request);

    }
}
