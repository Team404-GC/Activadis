using Activadis.Domain.Interfaces;

namespace Activadis.Domain.Entities
{
    public class Leaderboard : IEntity
    {
        public Guid Id { get; set; }
        public Guid? CategoryId { get; set; }
        public bool IsOverall { get; set; } = false;

        public List<LeaderboardEntry> Entries { get; set; } = [];

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }

        public Category? Category { get; set; }
    }

    public class LeaderboardEntry
    {
        public Guid UserId { get; set; }
        public required string FullName { get; set; }
        public string? JobTitle { get; set; }
        public double Rating { get; set; }
        public int MatchCount { get; set; }
        public double PeakRating { get; set; }
        public int Rank { get; set; }
        public string? RankTitle { get; set; }
    }
}
