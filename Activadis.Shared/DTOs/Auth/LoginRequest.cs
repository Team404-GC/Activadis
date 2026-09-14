using System.ComponentModel.DataAnnotations;

namespace Activadis.Shared.DTOs.Auth
{
    public class LoginRequest
    {
        [Required(ErrorMessage = "De e-mail moet ingevuld worden.")]
        public string Email { get; set; } = string.Empty;
        [Required(ErrorMessage = "Het wachtwoord moet ingevuld worden.")]
        public string Password { get; set; } = string.Empty;
    }
}
