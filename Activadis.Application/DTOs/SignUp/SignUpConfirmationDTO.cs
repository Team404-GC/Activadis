using Activadis.Shared.DTOs.SignUp;

namespace Activadis.Application.DTOs.SignUp
{
    public class SignUpConfirmationDTO
    {
        public SignUpRequest Request { get; set; }
        public DateTime Expires { get; set; }

        public SignUpConfirmationDTO(SignUpRequest request, DateTime expires)
        {
            Request = request;
            Expires = expires;
        }

        public bool HasExpired()
            => DateTime.UtcNow >= Expires;
    }
}
