using Activadis.Shared.DTOs.SignUp;
using Activadis.Shared.DTOs;

namespace Activadis.UI.Application.Interfaces
{
    public interface ISignUpService
    {
        Task<ApiResponse<object>> SignUpAsync(SignUpRequest request);
    }
}
