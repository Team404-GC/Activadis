using Activadis.Shared.DTOs.Auth;

namespace Activadis.Application.Interfaces
{
    public interface IAuthService
    {
        Task<Token> LoginAsync(LoginRequest request);
        Task<bool> IsActiveAsync(Guid userId);
        Task CheckPasswordSetupAsync(string token);
        Task SetPasswordAsync(SetPasswordRequest request);
    }
}
