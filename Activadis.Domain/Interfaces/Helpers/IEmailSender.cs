using Activadis.Domain.Models;

namespace Activadis.Domain.Interfaces.Helpers
{
    public interface IEmailSender
    {
        Task<bool> SendAsync(EmailMessage message);
    }
}
