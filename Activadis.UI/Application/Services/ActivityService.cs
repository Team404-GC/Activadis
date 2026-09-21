using Activadis.Shared.DTOs;
using Activadis.Shared.DTOs.Activity;
using Activadis.UI.Application.DTOs.Activity;
using Activadis.UI.Application.Interfaces;

namespace Activadis.UI.Application.Services
{
    public class ActivityService : IActivityService
    {
        private readonly IHttpService HttpService;

        public ActivityService(IHttpService httpService)
        {
            HttpService = httpService;
        }

        public async Task<ApiResponse<object>> CreateAsync(CreateActivityRequest request)
            => await HttpService.PostIncludeFileAsync<object, CreateActivityRequest>("/Activity", request, x => x.Image);

        public async Task<ApiResponse<IEnumerable<ActivityOverviewResponse>>> GetUpcomingAsync()
            => await HttpService.GetAsync<IEnumerable<ActivityOverviewResponse>>("/Activity");

        public async Task<ApiResponse<ActivityDetailResponse>> GetDetailAsync(Guid id)
            => await HttpService.GetAsync<ActivityDetailResponse>($"/Activity/{id}");

        public async Task<ApiResponse<IEnumerable<ActivityOverviewResponse>>> GetSignedUpAsync()
            => await HttpService.GetAsync<IEnumerable<ActivityOverviewResponse>>("/Activity/SignedUp");
    }
}
