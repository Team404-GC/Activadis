using System.ComponentModel.DataAnnotations;

namespace Activadis.UI.Application.DTOs.Activity
{
    public class EditActivityRequest
    {
        [Required(ErrorMessage = "De naam moet ingevuld worden.")]
        public string Name { get; set; } = string.Empty;
        [Required(ErrorMessage = "De beschrijving moet ingevuld worden.")]
        public string Description { get; set; } = string.Empty;
        public string? Requirements { get; set; }
        public decimal? CostPerPerson { get; set; }

        [Required(ErrorMessage = "De locatie moet ingevuld worden.")]
        public string Location { get; set; } = string.Empty;

        public int? MinParticipants { get; set; }
        public int? MaxParticipants { get; set; }

        public bool FoodIncluded { get; set; }
        public bool IsDraft { get; set; } = true;

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public DateTime SignUpDeadline { get; set; }
        public DateTime SignOutDeadline { get; set; }
        public string ActivityId { get; set; } = string.Empty;
    }
}
