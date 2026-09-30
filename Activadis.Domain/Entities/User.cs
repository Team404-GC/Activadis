using Activadis.Domain.Interfaces;
using Activadis.Domain.Enums;

namespace Activadis.Domain.Entities
{
    public class User : IEntity
    {
        public Guid Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string HashedPassword { get; set; } = string.Empty;
        public UserRole Role { get; set; }
        public string? JobTitle { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }

        // Navigation Properties
        public ICollection<SignUp> SignUps { get; set; } = [];
    }
}
