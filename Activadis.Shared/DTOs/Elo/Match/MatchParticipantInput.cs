namespace Activadis.Shared.DTOs.Elo.Match
{
    public class MatchParticipantInput
    {
        public Guid UserId { get; set; }

        public int Placement { get; set; }

        public bool IsTie { get; set; } = false;

        public Guid? TeamId { get; set; }
    }
}
