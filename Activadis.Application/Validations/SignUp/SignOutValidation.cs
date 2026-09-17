using Entities = Activadis.Domain.Entities;

namespace Activadis.Application.Validations.SignUp
{
    public static class SignOutValidation
    {
        public static void Validate(Entities.Activity? activity)
        {
            if (activity is null)
                throw new ArgumentException("Er is geen activiteit gevonden.");

            if (DateTime.UtcNow >= activity.SignOutDeadline)
                throw new ArgumentException("De uitschrijfdatum is al geweest.");
        }
    }
}
