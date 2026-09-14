using Activadis.Shared.DTOs.Auth;
using Activadis.Shared.DTOs;

namespace Activadis.UI.Application.Interfaces
{
	public interface IAuthService
	{
		Task<ApiResponse<Token>> LoginAsync(LoginRequest request);
		Task LogoutAsync();
	}
}
