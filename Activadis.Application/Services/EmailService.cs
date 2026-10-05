using Activadis.Domain.Interfaces.Helpers;
using Activadis.Application.Interfaces;
using Activadis.Application.Templates;

namespace Activadis.Application.Services
{
    public class EmailService : IEmailService
    {
        private const string ExampleLink = "https://localhost/wachtwoord-instellen?voorbeeld=true";

        private readonly IEmailSender EmailSender;

        public EmailService(IEmailSender emailSender)
        {
            EmailSender = emailSender;
        }

        public async Task<bool> SendSignUpConfirmationAsync(string email, string fullName, string token)
            => await EmailSender.SendAsync(EmailTemplates.SignUpConfirmation(email, fullName, token));

        public async Task<bool> SendPasswordSetupAsync(string email, string fullName, string link)
            => await EmailSender.SendAsync(EmailTemplates.PasswordSetup(email, fullName, link));

        public async Task<bool> SendTestAsync(string email, string fullName)
            => await SendPasswordSetupAsync(email, fullName, ExampleLink);
    }
}
