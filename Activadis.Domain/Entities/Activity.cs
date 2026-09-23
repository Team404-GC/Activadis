using Activadis.Domain.Interfaces;

namespace Activadis.Domain.Entities
{
    public class Activity : IEntity
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal CostPerPerson { get; set; }

        public string Location { get; set; } = string.Empty;
        public byte[] Image { get; set; } = [];
        public string ImageContentType { get; set; } = string.Empty;

        public int MinParticipants { get; set; }
        public int MaxParticipants { get; set; }

        public bool FoodIncluded { get; set; }
        public bool ExternalAllowed { get; set; }
        public bool PlusOneAllowed { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public DateTime SignUpDeadline { get; set; }
        public DateTime SignOutDeadline { get; set; }

        public int TotalSignUps => SignUps.Count;

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
        
        // Navigation Properties
        public ICollection<SignUp> SignUps { get; set; } = [];
    }
}
