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

        public async Task<bool> SendPasswordSetupAsync(string email, string fullName, string link)
            => await EmailSender.SendAsync(EmailTemplates.PasswordSetup(email, fullName, link));

        /// <summary>
        /// Sends the password setup email with an example link, to check that the email
        /// settings work and to see what the email looks like.
        /// </summary>
        public async Task<bool> SendTestAsync(string email, string fullName)
            => await SendPasswordSetupAsync(email, fullName, ExampleLink);
    }
}
