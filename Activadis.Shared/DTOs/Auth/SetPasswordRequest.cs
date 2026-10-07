using System.ComponentModel.DataAnnotations;

namespace Activadis.Shared.DTOs.Auth
{
    public class SetPasswordRequest
    {
        public const int MinPasswordLength = 8;

        // At least one capital letter, one digit and one character that is not a letter or digit.
        public const string PasswordPattern = "^(?=.*[A-Z])(?=.*[0-9])(?=.*[^a-zA-Z0-9]).*$";

        public string Token { get; set; } = string.Empty;

        [Required(ErrorMessage = "Het wachtwoord moet ingevuld worden.")]
        [MinLength(MinPasswordLength, ErrorMessage = "Het wachtwoord moet minimaal 8 tekens bevatten.")]
        [RegularExpression(PasswordPattern, ErrorMessage = "Het wachtwoord moet minimaal 1 hoofdletter, 1 cijfer en 1 speciaal teken bevatten.")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Het wachtwoord moet herhaald worden.")]
        [Compare(nameof(Password), ErrorMessage = "De wachtwoorden komen niet overeen.")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
