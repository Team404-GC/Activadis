using Activadis.Domain.Interfaces;

namespace Activadis.Domain.Entities
{
    public class Category : IEntity
    {
        public Guid Id { get; set; }
        public required string Name { get; set; }
        public string? Description { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }

        public ICollection<Rating> Ratings { get; set; } = [];
        public ICollection<Match> Matches { get; set; } = [];
    }
}
