using Activadis.Shared.DTOs.SignUp;

namespace Activadis.Application.Interfaces
{
    public interface ISignUpService
    {
        Task SignUpAsync(SignUpRequest request, Guid userId);
        Task SignOutAsync(Guid activityId, Guid userId);
    }
}
