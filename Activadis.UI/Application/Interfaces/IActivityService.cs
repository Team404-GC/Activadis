using Activadis.Shared.DTOs;
using Activadis.Shared.DTOs.Activity;
using Activadis.Shared.DTOs.Elo.Stats;
using Activadis.UI.Application.DTOs.Activity;

namespace Activadis.UI.Application.Interfaces
{
	public interface IActivityService
	{
		Task<ApiResponse<object>> CreateAsync(CreateActivityRequest request);
		Task<ApiResponse<object>> EditAsync(EditActivityRequest request);

        Task<ApiResponse<IEnumerable<ActivityOverviewResponse>>> GetActivitiesAsync(ActivityFilterRequest filter);
		Task<ApiResponse<ActivityDetailResponse>> GetDetailAsync(string identifier);
		Task<ApiResponse<IEnumerable<ActivityOverviewResponse>>> GetSignedUpAsync();
		string GetPhotoUrl(ActivityOverviewResponse activity);

		Task<ApiResponse<object>> UpdateCategoriesAsync(Guid id, UpdateActivityCategoriesRequest request);
		Task<ApiResponse<IEnumerable<ActivityParticipantResponse>>> GetParticipantsAsync(Guid id);
		Task<ApiResponse<IEnumerable<RecentMatchDto>>> GetLinkableMatchesAsync(Guid id);
		Task<ApiResponse<object>> LinkMatchAsync(Guid id, LinkMatchRequest request);
		Task<ApiResponse<object>> UnlinkMatchAsync(Guid id, Guid matchId);
	}
}
