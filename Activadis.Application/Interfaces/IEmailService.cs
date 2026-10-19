namespace Activadis.Application.Interfaces
{
    public interface IEmailService
    {
        Task<bool> SendAddWaitlistAsync(string email, string fullName, int position);
        Task<bool> SendSignUpConfirmationAsync(string email, string fullName, string token);
        Task<bool> SendSignOutConfirmationAsync(string email, string fullName, string token);
        Task<bool> SendPasswordSetupAsync(string email, string fullName, string link);
        Task<bool> SendTestAsync(string email, string fullName);
    }
}
