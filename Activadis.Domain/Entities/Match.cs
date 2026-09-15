using Activadis.Domain.Interfaces;

namespace Activadis.Domain.Entities
{
    public class Match : IEntity
    {
        public Guid Id { get; set; }
        public Guid CategoryId { get; set; }
        public DateTime MatchDate { get; set; }

        public string? Notes { get; set; }

        public bool IsFinalized { get; set; } = false;

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }

        public Category? Category { get; set; }
        public ICollection<MatchParticipant> Participants { get; set; } = [];
    }
}
