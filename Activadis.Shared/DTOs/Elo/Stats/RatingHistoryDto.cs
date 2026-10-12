namespace Activadis.Shared.DTOs.Elo.Stats
{
    public class PlayerRatingHistoryDto
    {
        public Guid UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public List<RatingHistoryPointDto> Points { get; set; } = [];
    }

    public class RatingHistoryPointDto
    {
        public DateTime Date { get; set; }
        public double Rating { get; set; }
    }
}