namespace Activadis.Shared.DTOs.Activity
{
    public class ActivityParticipantResponse
    {
        public Guid UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
    }
}
