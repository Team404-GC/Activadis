using Activadis.Application.DTOs.Activity;
using Activadis.Shared.DTOs.Elo.Stats;
using Activadis.Shared.DTOs.Activity;
using Activadis.Domain.Enums;

namespace Activadis.Application.Interfaces
{
	public interface IActivityService
	{
		Task CreateAsync(CreateActivityRequest request);
		Task EditAsync(EditActivityRequest request);

        Task<IEnumerable<ActivityOverviewResponse>> GetActivitiesAsync(Guid userId, UserRole role, ActivityFilterRequest request);

		Task<ActivityDetailResponse> GetDetailAsync(string identifier, Guid userId, bool isAdmin);
		Task<ActivityPhotoResponse> GetPhotoAsync(Guid id);
		Task<ActivityPhotoResponse> GetPhotoByIdAsync(Guid id, Guid photoId);
		Task AddPhotosAsync(Guid id, AddActivityPhotosRequest request);
		Task UpdatePhotoOrderAsync(Guid id, UpdateActivityPhotoOrderRequest request);
		Task DeletePhotoAsync(Guid id, Guid photoId);
		Task<IEnumerable<ActivityOverviewResponse>> GetSignedUpAsync(Guid userId);

		Task UpdateCategoriesAsync(Guid id, UpdateActivityCategoriesRequest request);
		Task<IEnumerable<ActivityParticipantResponse>> GetParticipantsAsync(Guid id);
		Task<InformParticipantsResponse> InformParticipantsAsync(Guid id);
		Task<IEnumerable<RecentMatchDto>> GetLinkableMatchesAsync(Guid id);
		Task LinkMatchAsync(Guid id, LinkMatchRequest request);
		Task UnlinkMatchAsync(Guid id, Guid matchId);
	}
}
