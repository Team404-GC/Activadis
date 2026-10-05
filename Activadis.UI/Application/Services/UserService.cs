using Activadis.UI.Application.Interfaces;
using Activadis.Shared.DTOs.User;
using Activadis.Shared.DTOs;

namespace Activadis.UI.Application.Services
{
    public class UserService : IUserService
    {
        private readonly IHttpService HttpService;

        public UserService(IHttpService httpService)
        {
            HttpService = httpService;
        }

        public async Task<ApiResponse<object>> CreateAsync(CreateUserRequest request)
            => await HttpService.PostAsync<object, CreateUserRequest>("/User", request);
    }
}
