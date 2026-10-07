using Activadis.Shared.DTOs;
using Activadis.Shared.DTOs.Activity;
using Activadis.UI.Application.DTOs.Activity;

namespace Activadis.UI.Application.Interfaces
{
	public interface IActivityService
	{
		Task<ApiResponse<object>> CreateAsync(CreateActivityRequest request);
		Task<ApiResponse<IEnumerable<ActivityOverviewResponse>>> GetActivitiesAsync(ActivityFilterRequest filter);
		Task<ApiResponse<ActivityDetailResponse>> GetDetailAsync(Guid id);
		Task<ApiResponse<IEnumerable<ActivityOverviewResponse>>> GetSignedUpAsync();
	}
}
