using Activadis.Application.DTOs;
using Activadis.UI.Application;
using Activadis.UI.Authentication;

namespace Activadis.UI.Application.Services
{
	public class AuthService : IAuthService
	{
		private readonly AuthStateProvider _authStateProvider;
		private readonly IHttpService _httpService;

		public AuthService(AuthStateProvider authStateProvider, IHttpService httpService)
		{
			_authStateProvider = authStateProvider;
			_httpService = httpService;
		}

		public async Task<ApiResponse<Token>> LoginAsync(LoginRequest request)
		{
			var response = await _httpService.PostAsync<Token, LoginRequest>("Auth/Login", request);

			if (response.Succeeded && response.Value is not null)
				await _authStateProvider.MarkUserAsAuthenticatedAsync(response.Value.JWT);

			return response;
		}

		public async Task LogoutAsync()
		{
			await _authStateProvider.MarkUserAsLoggedOutAsync();
		}
	}
}
