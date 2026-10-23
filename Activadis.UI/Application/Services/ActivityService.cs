using Activadis.Shared.DTOs;
using Activadis.Shared.DTOs.Activity;
using Activadis.Shared.DTOs.Elo.Stats;
using Activadis.UI.Application.DTOs.Activity;
using Activadis.UI.Application.Interfaces;

namespace Activadis.UI.Application.Services
{
	public class ActivityService : IActivityService
	{
		private readonly IHttpService HttpService;
		private readonly string ApiBaseUrl;

		public ActivityService(IHttpService httpService, IConfiguration configuration)
		{
			HttpService = httpService;
			ApiBaseUrl = configuration.GetValue<string>("ApiBaseUrl")
				?? throw new InvalidOperationException("The API base url was not found.");
		}

		public async Task<ApiResponse<object>> CreateAsync(CreateActivityRequest request)
			=> await HttpService.PostIncludeFileAsync<object, CreateActivityRequest>("/Activity", request, x => x.Image);

        public async Task<ApiResponse<object>> EditAsync(EditActivityRequest request)
            => await HttpService.PutIncludeFileAsync<object, EditActivityRequest>("/Activity", request, x => x.Image);

        public async Task<ApiResponse<IEnumerable<ActivityOverviewResponse>>> GetActivitiesAsync(ActivityFilterRequest filter)
			=> await HttpService.QueryAsync<IEnumerable<ActivityOverviewResponse>, ActivityFilterRequest>("/Activity", filter);

		public async Task<ApiResponse<ActivityDetailResponse>> GetDetailAsync(Guid id)
			=> await HttpService.GetAsync<ActivityDetailResponse>($"/Activity/{id}");

		public async Task<ApiResponse<IEnumerable<ActivityOverviewResponse>>> GetSignedUpAsync()
			=> await HttpService.GetAsync<IEnumerable<ActivityOverviewResponse>>("/Activity/SignedUp");

		public async Task<ApiResponse<object>> UpdateCategoriesAsync(Guid id, UpdateActivityCategoriesRequest request)
			=> await HttpService.PutAsync<object, UpdateActivityCategoriesRequest>($"/Activity/{id}/categories", request);

		public async Task<ApiResponse<IEnumerable<ActivityParticipantResponse>>> GetParticipantsAsync(Guid id)
			=> await HttpService.GetAsync<IEnumerable<ActivityParticipantResponse>>($"/Activity/{id}/participants");

		public async Task<ApiResponse<IEnumerable<RecentMatchDto>>> GetLinkableMatchesAsync(Guid id)
			=> await HttpService.GetAsync<IEnumerable<RecentMatchDto>>($"/Activity/{id}/matches/linkable");

		public async Task<ApiResponse<object>> LinkMatchAsync(Guid id, LinkMatchRequest request)
			=> await HttpService.PostAsync<object, LinkMatchRequest>($"/Activity/{id}/matches", request);

		public async Task<ApiResponse<object>> UnlinkMatchAsync(Guid id, Guid matchId)
			=> await HttpService.DeleteAsync<object>($"/Activity/{id}/matches/{matchId}");

		public string GetPhotoUrl(ActivityOverviewResponse activity)
			=> $"{ApiBaseUrl}/Activity/{activity.Id}/Photo?v={activity.PhotoVersion}";
	}
}
