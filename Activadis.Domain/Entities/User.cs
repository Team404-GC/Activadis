using Activadis.Domain.Interfaces;
using Activadis.Domain.Enums;

namespace Activadis.Domain.Entities
{
    public class User : IEntity
    {
        public Guid Id { get; set; }
        public required string FullName { get; set; }
        public required string Email { get; set; }
        public required string HashedPassword { get; set; }
        public UserRole Role { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
    }
}
