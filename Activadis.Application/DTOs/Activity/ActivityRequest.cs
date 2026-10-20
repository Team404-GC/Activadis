using Microsoft.AspNetCore.Http;

namespace Activadis.Application.DTOs.Activity
{
    public class ActivityRequest
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal CostPerPerson { get; set; }

        public string Location { get; set; } = string.Empty;
        public virtual IFormFile? Image { get; set; } = null;

        public int MinParticipants { get; set; }
        public int MaxParticipants { get; set; }

        public bool FoodIncluded { get; set; }
        public bool IsDraft { get; set; } = true;

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public DateTime SignUpDeadline { get; set; }
        public DateTime SignOutDeadline { get; set; }
    }
}
