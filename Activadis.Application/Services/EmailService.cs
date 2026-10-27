using Activadis.Domain.Interfaces.Helpers;
using Activadis.Application.Interfaces;
using Activadis.Application.Templates;
using Activadis.Domain.Entities;

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

        public async Task<bool> SendAddWaitlistConfirmationAsync(string email, string fullName, int position)
            => await EmailSender.SendAsync(EmailTemplates.AddWaitlistConfirmation(email, fullName, position));

        public async Task<bool> SendRemoveWaitlistConfirmationAsync(string email, string fullName, string token)
            => await EmailSender.SendAsync(EmailTemplates.RemoveWaitlistConfirmation(email, fullName, token));

        public async Task<bool> SendResignUpConfirmationAsync(string email, string fullName, string token)
            => await EmailSender.SendAsync(EmailTemplates.ResignUpConfirmation(email, fullName, token));

        public async Task<bool> SendSignUpConfirmationAsync(string email, string fullName, string token)
            => await EmailSender.SendAsync(EmailTemplates.SignUpConfirmation(email, fullName, token));

        public async Task<bool> SendEditSignUpConfirmationAsync(string email, string fullName, string token)
            => await EmailSender.SendAsync(EmailTemplates.EditSignUpConfirmation(email, fullName, token));

        public async Task<bool> SendSignOutConfirmationAsync(string email, string fullName, string token)
            => await EmailSender.SendAsync(EmailTemplates.SignOutConfirmation(email, fullName, token));

        public async Task<bool> SendPasswordSetupAsync(string email, string fullName, string token)
            => await EmailSender.SendAsync(EmailTemplates.PasswordSetup(email, fullName, token));

        public async Task<bool> SendActivityChangedAsync(string email, string fullName, Activity activity)
            => await EmailSender.SendAsync(EmailTemplates.ActivityChanged(email, fullName, activity));

        public async Task<bool> SendTestAsync(string email, string fullName)
            => await SendPasswordSetupAsync(email, fullName, ExampleLink);
    }
}
