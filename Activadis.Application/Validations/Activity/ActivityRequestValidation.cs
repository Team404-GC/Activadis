using Entities = Activadis.Domain.Entities;
using Activadis.Application.DTOs.Activity;

namespace Activadis.Application.Validations.Activity
{
    public static class ActivityRequestValidation
    {
        public static void Validate(this ActivityRequest request, Entities.Activity? activity = null)
        {
            if (request.Name is null || string.IsNullOrWhiteSpace(request.Name))
                throw new ArgumentException("De naam moet ingevuld worden.");

            if (request.Description is null || string.IsNullOrWhiteSpace(request.Description))
                throw new ArgumentException("De beschrijving moet ingevuld worden.");

            if (request.Location is null || string.IsNullOrWhiteSpace(request.Location))
                throw new ArgumentException("De locatie moet ingevuld worden.");

            if (request.CostPerPerson < 0)
                throw new ArgumentException("De kosten per persoon kan niet negatief zijn.");

            if (request.MinParticipants < 0)
                throw new ArgumentException("Het minimaal aantal deelnemers kan niet negatief zijn.");

            if (request.MaxParticipants < 0)
                throw new ArgumentException("Het maximaal aantal deelnemers kan niet negatief zijn.");

            if (request.StartDate == DateTime.MinValue)
                throw new ArgumentException("De startdatum moet ingevuld worden.");

            if (DateTime.UtcNow >= request.StartDate && (activity?.StartDate ?? DateTime.MinValue) != request.StartDate)
                throw new ArgumentException("De startdatum mag niet al geweest zijn.");

            if (request.EndDate == DateTime.MinValue)
                throw new ArgumentException("De einddatum moet ingevuld worden.");

            if (DateTime.UtcNow >= request.EndDate && (activity?.EndDate ?? DateTime.MinValue) != request.EndDate)
                throw new ArgumentException("De einddatum mag niet al geweest zijn.");

            if (request.StartDate >= request.EndDate)
                throw new ArgumentException("De einddatum moet na de startdatum liggen.");

            if (request.SignUpDeadline == DateTime.MinValue)
                throw new ArgumentException("De inschrijfdatum moet ingevuld worden.");

            if (DateTime.UtcNow >= request.SignUpDeadline && (activity?.SignUpDeadline ?? DateTime.MinValue) != request.SignUpDeadline)
                throw new ArgumentException("De inschrijfdatum mag niet al geweest zijn.");

            if (request.SignOutDeadline == DateTime.MinValue)
                throw new ArgumentException("De uitschrijfdatum moet ingevuld worden.");

            if (DateTime.UtcNow >= request.SignOutDeadline && (activity?.SignOutDeadline ?? DateTime.MinValue) != request.SignOutDeadline)
                throw new ArgumentException("De uitschrijfdatum mag niet al geweest zijn.");
        }
    }
}
