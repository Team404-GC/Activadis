using Activadis.Shared.DTOs.Auth;

namespace Activadis.Application.Interfaces
{
    public interface IAuthService
    {
        Task<Token> LoginAsync(LoginRequest request);
    }
}
