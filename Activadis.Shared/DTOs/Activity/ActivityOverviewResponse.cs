namespace Activadis.Shared.DTOs.Activity
{
    public class ActivityOverviewResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;

        public string Location { get; set; } = string.Empty;

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}
