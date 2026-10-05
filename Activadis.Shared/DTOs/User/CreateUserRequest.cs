using System.ComponentModel.DataAnnotations;

namespace Activadis.Shared.DTOs.User
{
    public class CreateUserRequest
    {
        [Required(ErrorMessage = "De naam moet ingevuld worden.")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "De e-mail moet ingevuld worden.")]
        [EmailAddress(ErrorMessage = "De e-mail is ongeldig.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "De functie moet ingevuld worden.")]
        public string JobTitle { get; set; } = string.Empty;

        [Required(ErrorMessage = "De rol moet gekozen worden.")]
        public string Role { get; set; } = "User";
    }
}
