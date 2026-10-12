using Activadis.Shared.DTOs;
using Activadis.Shared.DTOs.Elo;
using Activadis.Shared.DTOs.Elo.Category;
using Activadis.Shared.DTOs.Elo.Leaderboard;
using Activadis.Shared.DTOs.Elo.Match;
using Activadis.Shared.DTOs.Elo.Stats;
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
            => await HttpService.PutAsync<CategoryDto, UpdateCategoryRequest>($"/Elo/categories/{request.Id}", request);

        public async Task<ApiResponse<List<PlayerRatingHistoryDto>>> GetRatingHistoryAsync(Guid categoryId, int top = 5, List<Guid>? userIds = null)
        {
            var url = $"/Elo/stats/rating-history?categoryId={categoryId}&top={top}";
            if (userIds is { Count: > 0 })
                url += string.Concat(userIds.Select(id => $"&userIds={id}"));

            return await HttpService.GetAsync<List<PlayerRatingHistoryDto>>(url);
        }

        public async Task<ApiResponse<EloSummaryDto>> GetSummaryAsync(Guid? categoryId = null)
            => await HttpService.GetAsync<EloSummaryDto>(
                categoryId.HasValue ? $"/Elo/stats/summary?categoryId={categoryId}" : "/Elo/stats/summary");

        public async Task<ApiResponse<List<ActivityPointDto>>> GetActivityAsync(Guid? categoryId = null, int months = 12)
            => await HttpService.GetAsync<List<ActivityPointDto>>(
                categoryId.HasValue
                    ? $"/Elo/stats/activity?categoryId={categoryId}&months={months}"
                    : $"/Elo/stats/activity?months={months}");

        public async Task<ApiResponse<List<RecentMatchDto>>> GetRecentMatchesAsync(Guid? categoryId = null, int take = 20)
            => await HttpService.GetAsync<List<RecentMatchDto>>(
                categoryId.HasValue
                    ? $"/Elo/matches/recent?categoryId={categoryId}&take={take}"
                    : $"/Elo/matches/recent?take={take}");

    }
}
