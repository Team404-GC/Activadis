using Activadis.Domain.Entities;

namespace Activadis.Application.Interfaces
{
    public interface IEmailService
    {
        Task<bool> SendAddWaitlistConfirmationAsync(string email, string fullName, int position);
        Task<bool> SendRemoveWaitlistConfirmationAsync(string email, string fullName, string token);
        Task<bool> SendResignUpConfirmationAsync(string email, string fullName, string token);
        Task<bool> SendSignUpConfirmationAsync(string email, string fullName, string token);
        Task<bool> SendEditSignUpConfirmationAsync(string email, string fullName, string token);
        Task<bool> SendSignOutConfirmationAsync(string email, string fullName, string token);
        Task<bool> SendPasswordSetupAsync(string email, string fullName, string link);
        Task<bool> SendActivityChangedAsync(string email, string fullName, Activity activity);
        Task<bool> SendActivityCancelledAsync(string email, string fullName, Activity activity);
        Task<bool> SendTestAsync(string email, string fullName);
    }
}
