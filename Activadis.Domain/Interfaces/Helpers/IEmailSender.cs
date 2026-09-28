using Activadis.Domain.Models;

namespace Activadis.Domain.Interfaces.Helpers
{
    public interface IEmailSender
    {
        /// <summary>
        /// Sends an email. Returns false when sending failed. The failure is logged and
        /// never thrown, so a problem with the mail server cannot break the request that
        /// triggered the email.
        /// </summary>
        Task<bool> SendAsync(EmailMessage message);
    }
}
