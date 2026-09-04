using Activadis.Application.DTOs.Auth;
using Activadis.Application.DTOs;

namespace Activadis.UI.Application.Interfaces
{
	public interface IAuthService
	{
		Task<ApiResponse<Token>> LoginAsync(LoginRequest request);
		Task LogoutAsync();
	}
}
