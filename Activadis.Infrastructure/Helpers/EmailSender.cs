using Activadis.Domain.Interfaces.Helpers;
using Microsoft.Extensions.Logging;
using Activadis.Domain.Models;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

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

                await client.SendAsync(ToMimeMessage(message));
                await client.DisconnectAsync(true);

                return true;
            }
            catch (Exception exception)
            {
                Logger.LogError(exception, "Sending the email \"{Subject}\" to {Recipient} failed.", message.Subject, message.ToEmail);
                return false;
            }
        }

        private MimeMessage ToMimeMessage(EmailMessage message)
        {
            MimeMessage mimeMessage = new MimeMessage();
            mimeMessage.From.Add(new MailboxAddress(Settings.SenderName, Settings.SenderEmail));
            mimeMessage.To.Add(new MailboxAddress(message.ToName, message.ToEmail));
            mimeMessage.Subject = message.Subject;

            BodyBuilder body = new BodyBuilder()
            {
                HtmlBody = message.HtmlBody,
                TextBody = message.TextBody
            };

            mimeMessage.Body = body.ToMessageBody();
            return mimeMessage;
        }
    }
}
