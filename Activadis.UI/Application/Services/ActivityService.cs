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

        public async Task CreateAsync(CreateActivityRequest request)
        {
            await HttpService.PostAsync<object, CreateActivityRequest>("/Activity", request);
        }
    }
}
