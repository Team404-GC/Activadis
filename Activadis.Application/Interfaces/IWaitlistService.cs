using Activadis.Domain.Entities;
using Activadis.Shared.DTOs.SignUp;

namespace Activadis.Application.Interfaces
{
    public interface IWaitlistService
    {
        Task AddToWaitlistAsync(SignUpRequest request, SignUp? signUp);
        Task NotifyWaitlistAsync(Guid activityId);
    }
}
