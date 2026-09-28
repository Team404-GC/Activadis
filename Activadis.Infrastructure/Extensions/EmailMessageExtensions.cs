using Activadis.Infrastructure.Helpers;
using Activadis.Domain.Models;
using MimeKit;

namespace Activadis.Infrastructure.Extensions
{
    public static class EmailMessageExtensions
    {
        public static MimeMessage ToMimeMessage(this EmailMessage message, EmailSettings settings)
        {
            MimeMessage mimeMessage = new MimeMessage();
            mimeMessage.From.Add(new MailboxAddress(settings.SenderName, settings.SenderEmail));
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
