namespace Activadis.Shared.DTOs.Activity
{
    public class ActivitySignUpListResponse
    {
        public List<ActivitySignUpResponse> Participants { get; set; } = [];
        public List<ActivitySignUpResponse> Waitlist { get; set; } = [];
        public int TotalParticipants { get; set; }
        public int TotalPlusOnes { get; set; }
    }
}
