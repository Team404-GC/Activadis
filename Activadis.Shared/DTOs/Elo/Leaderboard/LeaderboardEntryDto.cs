namespace Activadis.Shared.DTOs.Elo.Leaderboard
{
    public class LeaderboardEntryDto
    {
        public Guid UserId { get; set; }
        public required string FullName { get; set; }
        public string? JobTitle { get; set; }
        public double Rating { get; set; }
        public int MatchCount { get; set; }
        public double PeakRating { get; set; }
        public int Rank { get; set; }
    }
}
