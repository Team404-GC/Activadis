using Activadis.Shared.DTOs.Elo.Match;

namespace Activadis.Shared.DTOs.Elo
{
    public class PlayerProfileDto
    {
        public Guid UserId { get; set; }
        public required string FullName { get; set; }
        public string? JobTitle { get; set; }
        public required string Email { get; set; }

        public double OverallRating { get; set; }

        public int TotalMatches { get; set; }

        public required List<RatingDto> CategoryRatings { get; set; }

        public required List<MatchHistoryDto> RecentMatches { get; set; }
    }
}
