namespace Activadis.Application.Interfaces
{
    public interface IEmailService
    {
        Task<bool> SendSignUpConfirmationAsync(string email, string fullName, string token);
        Task<bool> SendPasswordSetupAsync(string email, string fullName, string link);
        Task<bool> SendTestAsync(string email, string fullName);
    }
}
