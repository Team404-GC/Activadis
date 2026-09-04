using System.ComponentModel.DataAnnotations;

namespace Activadis.Application.DTOs.Auth
{
    public class LoginRequest
    {
        [Required(ErrorMessage = "De email moet ingevuld worden.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Het wachtwoord moet ingevuld worden.")]
        public string Password { get; set; } = string.Empty;
    }
}
