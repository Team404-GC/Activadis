using Activadis.Shared.DTOs.SignUp;
using Entities = Activadis.Domain.Entities;

namespace Activadis.Application.Validations.SignUp
{
    public static class SignOutRequestValidation
    {
        public static void Validate(this SignOutRequest request, Entities.Activity? activity)
        {
            if (string.IsNullOrWhiteSpace(request.Email))
                throw new ArgumentException("De e-mail moet ingevuld worden.");

            if (activity is null)
                throw new ArgumentException("Er is geen activiteit gevonden.");

            if (DateTime.UtcNow >= activity.SignOutDeadline)
                throw new ArgumentException("De uitschrijfdatum is al geweest.");
        }
    }
}
