using Activadis.Application.DTOs.Activity;

namespace Activadis.Application.Validations.Activity
{
    public static class CreateActivityRequestValidation
    {
        private static readonly List<string> ValidImageTypes = ["png", "jpg", "jpeg", "webp"];

        public static void Validate(this CreateActivityRequest request)
        {
            if (request.Image.Length <= 0)
                throw new ArgumentException("De foto heeft een ongeldig bestandsgrootte.");

            if (!ValidImageTypes.Contains(request.Image.FileName.Split('.').Last()))
                throw new ArgumentException("De foto heeft een ongeldig bestandstype.");

            if (request.StartDate == DateTime.MinValue)
                throw new ArgumentException("De startdatum moet ingevuld worden.");

            if (request.EndDate == DateTime.MinValue)
                throw new ArgumentException("De einddatum moet ingevuld worden.");

            if (request.StartDate == request.EndDate)
                throw new ArgumentException("De activiteit kan niet gelijk eindigen.");

            if (request.SignUpDeadline == DateTime.MinValue)
                throw new ArgumentException("De inschrijfdatum moet ingevuld worden.");

            if (request.SignOutDeadline == DateTime.MinValue)
                throw new ArgumentException("De uitschrijfdatum moet ingevuld worden.");

            if (request.SignUpDeadline == request.SignOutDeadline)
                throw new ArgumentException("De inschrijving kan niet gelijk eindigen.");
        }
    }
}
