namespace Activadis.Shared.DTOs.Elo.Stats
{
    public class RecentMatchDto
    {
        public Guid MatchId { get; set; }
        public Guid CategoryId { get; set; }
        public string? CategorySlug { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public DateTime MatchDate { get; set; }
        public string? Notes { get; set; }
        public List<RecentMatchParticipantDto> Participants { get; set; } = [];
    }

    public class RecentMatchParticipantDto
    {
        public Guid UserId { get; set; }
        public string? UserSlug { get; set; }
        public string FullName { get; set; } = string.Empty;
        public int Placement { get; set; }
        public double RatingChange { get; set; }
    }
}