using Activadis.Domain.Interfaces;

namespace Activadis.Domain.Entities
{
    public class MatchParticipant : IEntity
    {
        public Guid Id { get; set; }
        public Guid MatchId { get; set; }
        public Guid UserId { get; set; }

        public int Placement { get; set; }

        public double ActualScore { get; set; }

        public double ExpectedScore { get; set; }

        public double RatingChange { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }

        public Match? Match { get; set; }
        public User? User { get; set; }
    }
}
