namespace Activadis.Shared.DTOs.Elo
{
    public class ParticipantResultDto
    {
        public Guid UserId { get; set; }
        public required string FullName { get; set; }
        public int Placement { get; set; }
        public double OldRating { get; set; }
        public double NewRating { get; set; }
        public double RatingChange { get; set; }
        public double ActualScore { get; set; }
        public double ExpectedScore { get; set; }
        public Guid? TeamId { get; set; }
    }
}
