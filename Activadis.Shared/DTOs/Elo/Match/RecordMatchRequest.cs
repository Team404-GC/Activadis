namespace Activadis.Shared.DTOs.Elo.Match
{
    public class RecordMatchRequest
    {
        public Guid CategoryId { get; set; }
        public DateTime MatchDate { get; set; }
        public string? Notes { get; set; }

        public required string MatchType { get; set; } = "Individual";

        public required List<MatchParticipantInput> Participants { get; set; }
    }
}
