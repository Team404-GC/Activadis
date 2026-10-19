using Activadis.Shared.DTOs.SignUp;

namespace Activadis.Application.Interfaces
{
    public interface IWaitlistService
    {
        Task AddToWaitlistAsync(SignUpRequest request);
        Task NotifyWaitlistAsync(Guid activityId);
        Task RemoveFromWaitlistAsync(SignOutRequest request);
    }
}
