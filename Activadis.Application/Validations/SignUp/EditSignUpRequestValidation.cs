using Entities = Activadis.Domain.Entities;
using Activadis.Shared.DTOs.SignUp;

namespace Activadis.Application.Validations.SignUp
{
    public static class EditSignUpRequestValidation
    {
        public static void Validate(this EditSignUpRequest request, Entities.Activity? activity, Guid userId)
        {
            if (userId == Guid.Empty && string.IsNullOrWhiteSpace(request.Email))
                throw new ArgumentException("De e-mail moet ingevuld worden.");

            if (activity is null)
                throw new ArgumentException("Er is geen activiteit gevonden.");

            if (DateTime.UtcNow >= activity.SignOutDeadline)
                throw new ArgumentException("De uitschrijfdatum is al geweest, dit zorgt ervoor dat je niet meer kan wijzigen.");
        }
    }
}
