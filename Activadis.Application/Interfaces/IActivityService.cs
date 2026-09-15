using Activadis.Application.DTOs.Activity;
using Activadis.Shared.DTOs.Activity;

namespace Activadis.Application.Interfaces
{
    public interface IActivityService
    {
        Task CreateAsync(CreateActivityRequest request);
        Task<IEnumerable<ActivityOverviewResponse>> GetUpcomingAsync();
    }
}
