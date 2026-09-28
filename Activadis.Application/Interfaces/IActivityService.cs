using Activadis.Application.DTOs.Activity;
using Activadis.Shared.DTOs.Activity;
using Activadis.Domain.Enums;

namespace Activadis.Application.Interfaces
{
    public interface IActivityService
    {
        Task CreateAsync(CreateActivityRequest request);
        Task<IEnumerable<ActivityOverviewResponse>> GetUpcomingAsync(Guid userId, UserRole role);
        Task<ActivityDetailResponse> GetDetailAsync(Guid id, Guid userId);
        Task<IEnumerable<ActivityOverviewResponse>> GetSignedUpAsync(Guid userId);
    }
}
