using Activadis.Infrastructure.Extensions;
using Activadis.Domain.Interfaces.Helpers;
using Microsoft.Extensions.Logging;
using Activadis.Domain.Models;
using MailKit.Net.Smtp;
using MailKit.Security;

namespace Activadis.Infrastructure.Helpers
{
    public class EmailSender : IEmailSender
    {
        private const int TimeoutInMilliseconds = 10_000;

        private readonly EmailSettings Settings;
        private readonly ILogger<EmailSender> Logger;

        public EmailSender(EmailSettings settings, ILogger<EmailSender> logger)
        {
            Settings = settings;
            Logger = logger;
        }

        public async Task<bool> SendAsync(EmailMessage message)
        {
            try
            {
                using SmtpClient client = new SmtpClient();
                client.Timeout = TimeoutInMilliseconds;

                await client.ConnectAsync(Settings.Host, Settings.Port, SecureSocketOptions.Auto);

                if (!string.IsNullOrWhiteSpace(Settings.Username))
                    await client.AuthenticateAsync(Settings.Username, Settings.Password);

                await client.SendAsync(message.ToMimeMessage(Settings));
                await client.DisconnectAsync(true);

                return true;
            }
            catch (Exception exception)
            {
                Logger.LogError(exception, "Sending the email \"{Subject}\" to {Recipient} failed.", message.Subject, message.ToEmail);
                return false;
            }
        }
    }
}
