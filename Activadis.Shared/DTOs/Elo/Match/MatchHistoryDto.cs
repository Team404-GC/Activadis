namespace Activadis.Shared.DTOs.Elo.Match
{
    public class MatchHistoryDto
    {
        public Guid MatchId { get; set; }
        public required string CategoryName { get; set; }
        public DateTime MatchDate { get; set; }
        public int Placement { get; set; }
        public double RatingBefore { get; set; }
        public double RatingAfter { get; set; }
        public double RatingChange { get; set; }
    }
}
