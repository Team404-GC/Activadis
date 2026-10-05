using System.Text.RegularExpressions;
using Activadis.Shared.DTOs.Auth;

namespace Activadis.Application.Validations.Auth
{
    public static class SetPasswordRequestValidation
    {
        public static void Validate(this SetPasswordRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Password))
                throw new ArgumentException("Het wachtwoord moet ingevuld worden.");

            if (request.Password.Length < SetPasswordRequest.MinPasswordLength)
                throw new ArgumentException("Het wachtwoord moet minimaal 8 tekens bevatten.");

            if (!Regex.IsMatch(request.Password, SetPasswordRequest.PasswordPattern))
                throw new ArgumentException("Het wachtwoord moet minimaal 1 hoofdletter, 1 cijfer en 1 speciaal teken bevatten.");

            if (request.Password != request.ConfirmPassword)
                throw new ArgumentException("De wachtwoorden komen niet overeen.");
        }
    }
}
