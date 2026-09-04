using System.ComponentModel.DataAnnotations;

namespace Activadis.Application.DTOs
{
    public class LoginRequest
    {
        [Required(ErrorMessage = "email is required.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required.")]
        public string Password { get; set; } = string.Empty;
    }
}
