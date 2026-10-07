using Activadis.Shared.DTOs.Auth;
using Activadis.Shared.DTOs;

namespace Activadis.UI.Application.Interfaces
{
	public interface IAuthService
	{
		Task<ApiResponse<Token>> LoginAsync(LoginRequest request);
		Task<ApiResponse<object>> CheckPasswordSetupAsync(string token);
		Task<ApiResponse<object>> SetPasswordAsync(SetPasswordRequest request);
		Task LogoutAsync();
	}
}
