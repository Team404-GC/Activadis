using Microsoft.AspNetCore.Components.Forms;
using System.ComponentModel.DataAnnotations;

namespace Activadis.UI.Application.DTOs.Activity
{
    public class CreateActivityRequest
    {
        [Required(ErrorMessage = "De naam moet ingevuld worden.")]
        public string Name { get; set; } = string.Empty;
        [Required(ErrorMessage = "De beschrijving moet ingevuld worden.")]
        public string Description { get; set; } = string.Empty;
        public decimal CostPerPerson { get; set; }

        [Required(ErrorMessage = "De locatie moet ingevuld worden.")]
        public string Location { get; set; } = string.Empty;
        [Required(ErrorMessage = "Een foto moet geüpload worden.")]
        public IBrowserFile? Image { get; set; }

        public int MinParticipants { get; set; }
        public int MaxParticipants { get; set; }

        public bool FoodIncluded { get; set; }
        public bool ExternalAllowed { get; set; }
        public bool PlusOneAllowed { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public DateTime SignUpDeadline { get; set; }
        public DateTime SignOutDeadline { get; set; }
    }
}
