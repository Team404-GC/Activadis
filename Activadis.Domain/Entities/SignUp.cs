using Activadis.Domain.Interfaces;

namespace Activadis.Domain.Entities
{
    public class SignUp : IEntity
    {
        public Guid Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public bool HasPlusOne { get; set; }
        public bool IsExternal { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }

        // Navigation Keys
        public Guid ActivityId { get; set; }
        public Guid? UserId { get; set; }

        // Navigation Properties
        public Activity Activity { get; set; } = null!;
        public User? User { get; set; }
    }
}
