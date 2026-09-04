
using Activadis.Application.DTOs;

namespace Activadis.UI.Application
{
	public interface IAuthService
	{
		Task<ApiResponse<Token>> LoginAsync(LoginRequest request);
		Task LogoutAsync();
	}
}
