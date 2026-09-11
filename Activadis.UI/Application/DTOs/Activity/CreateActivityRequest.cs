using Microsoft.AspNetCore.Components.Forms;

namespace Activadis.UI.Application.DTOs.Activity
{
    public class CreateActivityRequest
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal CostPerPerson { get; set; }

        public string Location { get; set; } = string.Empty;
        public required IBrowserFile Image { get; set; }

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
