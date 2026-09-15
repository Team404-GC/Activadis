namespace Activadis.Shared.DTOs.Elo
{
    public class RatingDto
    {
        public Guid UserId { get; set; }
        public Guid CategoryId { get; set; }
        public required string CategoryName { get; set; }
        public double CurrentRating { get; set; }
        public int MatchCount { get; set; }
        public double PeakRating { get; set; }
    }
}
