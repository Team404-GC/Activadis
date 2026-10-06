using Activadis.Shared.DTOs.SignUp;

namespace Activadis.Application.DTOs
{
    public class ConfirmationDTO<TRequest>
    {
        public TRequest Request { get; set; }
        public DateTime Expires { get; set; }

        public ConfirmationDTO(TRequest request, DateTime expires)
        {
            Request = request;
            Expires = expires;
        }

        public bool HasExpired()
            => DateTime.UtcNow >= Expires;
    }
}
