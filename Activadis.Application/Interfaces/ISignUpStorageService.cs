using Activadis.Shared.DTOs.SignUp;

namespace Activadis.Application.Interfaces
{
    public interface ISignUpStorageService
    {
        Task SendSignUpConfirmationAsync(SignUpRequest request);
        Task UseSignUpConfirmationAsync(string token);
    }
}
