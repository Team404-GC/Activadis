using Activadis.Domain.Interfaces;

namespace Activadis.Domain.Entities
{
    public class WaitlistEntry : IEntity
    {
        public Guid Id { get; set; }
        public int Position { get; set; }
        public string Email { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }

        // Navigation Keys
        public Guid ActivityId { get; set; }

        // Navigation Properties
        public Activity Activity { get; set; } = null!;
    }
}
