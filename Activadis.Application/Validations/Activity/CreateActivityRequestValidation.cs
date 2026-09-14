using Activadis.Application.DTOs.Activity;

namespace Activadis.Application.Validations.Activity
{
    public static class CreateActivityRequestValidation
    {
        private static readonly List<string> ValidImageTypes = ["png", "jpg", "jpeg", "webp"];

        public static void Validate(this CreateActivityRequest request)
        {
            if (request.Name is null || string.IsNullOrWhiteSpace(request.Name))
                throw new ArgumentException("De naam moet ingevuld worden.");

            if (request.Description is null || string.IsNullOrWhiteSpace(request.Description))
                throw new ArgumentException("De beschrijving moet ingevuld worden.");

            if (request.Location is null || string.IsNullOrWhiteSpace(request.Location))
                throw new ArgumentException("De locatie moet ingevuld worden.");

            if (request.Image.Length <= 0)
                throw new ArgumentException("De foto heeft een ongeldige bestandsgrootte.");

            if (!ValidImageTypes.Contains(request.Image.FileName.Split('.').Last(), StringComparer.OrdinalIgnoreCase))
                throw new ArgumentException("De foto heeft een ongeldig bestandstype.");

            if (request.CostPerPerson < 0)
                throw new ArgumentException("De kosten per persoon kan niet negatief zijn.");

            if (request.MinParticipants < 0)
                throw new ArgumentException("Het minimaal aantal deelnemers kan niet negatief zijn.");

            if (request.MaxParticipants < 0)
                throw new ArgumentException("Het maximaal aantal deelnemers kan niet negatief zijn.");

            if (request.StartDate == DateTime.MinValue)
                throw new ArgumentException("De startdatum moet ingevuld worden.");

            if (request.EndDate == DateTime.MinValue)
                throw new ArgumentException("De einddatum moet ingevuld worden.");

            if (request.StartDate >= request.EndDate)
                throw new ArgumentException("De einddatum moet na de startdatum liggen.");

            if (request.SignUpDeadline == DateTime.MinValue)
                throw new ArgumentException("De inschrijfdatum moet ingevuld worden.");

            if (request.SignOutDeadline == DateTime.MinValue)
                throw new ArgumentException("De uitschrijfdatum moet ingevuld worden.");
        }
    }
}
