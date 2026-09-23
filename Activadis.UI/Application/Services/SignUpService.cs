using Activadis.UI.Application.Interfaces;
using Activadis.Shared.DTOs.SignUp;
using Activadis.Shared.DTOs;

namespace Activadis.UI.Application.Services
{
    public class SignUpService : ISignUpService
    {
        private readonly IHttpService HttpService;

        public SignUpService(IHttpService httpService)
        {
            HttpService = httpService;
        }

        public async Task<ApiResponse<object>> SignUpAsync(SignUpRequest request)
            => await HttpService.PostAsync<object, SignUpRequest>("/SignUp", request);

        public async Task<ApiResponse<object>> SignOutAsync(Guid activityId)
            => await HttpService.DeleteAsync<object>($"/SignUp/{activityId}");
    }
}
