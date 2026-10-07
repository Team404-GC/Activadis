using Entities = Activadis.Domain.Entities;
using System.ComponentModel.DataAnnotations;
using Activadis.Shared.DTOs.User;
using Activadis.Domain.Enums;

namespace Activadis.Application.Validations.User
{
    public static class CreateUserRequestValidation
    {
        public static void Validate(this CreateUserRequest request, Entities.User? existingUser)
        {
            if (string.IsNullOrWhiteSpace(request.FullName))
                throw new ArgumentException("De naam moet ingevuld worden.");

            if (string.IsNullOrWhiteSpace(request.Email))
                throw new ArgumentException("De e-mail moet ingevuld worden.");

            if (!new EmailAddressAttribute().IsValid(request.Email))
                throw new ArgumentException("De e-mail is ongeldig.");

            if (string.IsNullOrWhiteSpace(request.JobTitle))
                throw new ArgumentException("De functie moet ingevuld worden.");

            if (!Enum.TryParse(request.Role, out UserRole role) || !Enum.IsDefined(role))
                throw new ArgumentException("De rol is ongeldig.");

            if (existingUser is not null)
                throw new ArgumentException("Er bestaat al een account met deze e-mail.");
        }
    }
}
