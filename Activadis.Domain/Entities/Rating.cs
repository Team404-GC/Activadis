using Activadis.Domain.Interfaces;

namespace Activadis.Domain.Entities
{
    public class Rating : IEntity
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid CategoryId { get; set; }

        public double CurrentRating { get; set; } = 1200;

        public int MatchCount { get; set; } = 0;

        public double PeakRating { get; set; } = 1200;

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }

        public User? User { get; set; }
        public Category? Category { get; set; }
    }
}
