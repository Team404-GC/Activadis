using Activadis.UI.Application.Interfaces;
using Activadis.Application.DTOs.Auth;
using Activadis.UI.Authentication;
using Activadis.Application.DTOs;

namespace Activadis.UI.Application.Services
{
	public class AuthService : IAuthService
	{
		private readonly AuthStateProvider AuthStateProvider;
		private readonly IHttpService HttpService;

		public AuthService(AuthStateProvider authStateProvider, IHttpService httpService)
		{
			AuthStateProvider = authStateProvider;
			HttpService = httpService;
		}

		public async Task<ApiResponse<Token>> LoginAsync(LoginRequest request)
		{
			var response = await HttpService.PostAsync<Token, LoginRequest>("/Auth/Login", request);

			if (response.Succeeded && response.Value is not null)
				await AuthStateProvider.MarkUserAsAuthenticatedAsync(response.Value.JWT);

			return response;
		}

		public async Task LogoutAsync()
			=> await AuthStateProvider.MarkUserAsLoggedOutAsync();
	}
}
