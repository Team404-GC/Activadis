using Activadis.Shared.DTOs;
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
    }
}
