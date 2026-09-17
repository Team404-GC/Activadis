using Entities = Activadis.Domain.Entities;
using Activadis.Shared.DTOs.SignUp;

namespace Activadis.Application.Validations.SignUp
{
    public static class SignUpRequestValidation
    {
        public static void Validate(this SignUpRequest request, Entities.Activity? activity, Guid userId)
        {
            if (string.IsNullOrWhiteSpace(request.FullName))
                throw new ArgumentException("De naam moet ingevuld worden.");

            if (string.IsNullOrWhiteSpace(request.Email))
                throw new ArgumentException("De e-mail moet ingevuld worden.");

            if (activity is null)
                throw new ArgumentException("Er is geen activiteit gevonden.");

            if (!activity.ExternalAllowed && userId == Guid.Empty)
                throw new ArgumentException("Deze activiteit accepteert geen externe deelnemers.");

            if (!activity.PlusOneAllowed && request.HasPlusOne)
                throw new ArgumentException("Deze activiteit accepteert geen +1.");

            if (activity.SignUpDeadline <= DateTime.UtcNow)
                throw new ArgumentException("De inschrijfdatum is al geweest.");
        }
    }
}
