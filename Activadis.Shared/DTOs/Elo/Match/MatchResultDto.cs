using Activadis.Shared.DTOs.Elo;

namespace Activadis.Shared.DTOs.Elo.Match
{
    public class MatchResultDto
    {
        public Guid MatchId { get; set; }
        public Guid CategoryId { get; set; }
        public DateTime MatchDate { get; set; }
        public string? Notes { get; set; }
        public required string MatchType { get; set; }

        public required List<ParticipantResultDto> Results { get; set; }
    }
}
