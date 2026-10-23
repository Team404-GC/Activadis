namespace Activadis.Shared.DTOs.Activity
{
    public class ActivityOverviewResponse
    {
        public Guid Id { get; set; }
        public string? Slug { get; set; }
        public string Name { get; set; } = string.Empty;

		public string Location { get; set; } = string.Empty;
		public bool HasPhoto { get; set; }
		public long PhotoVersion { get; set; }

		public bool IsDraft { get; set; }
		public bool IsDeleted { get; set; }
		public bool HasTakenPlace { get; set; }
		public bool HasSignedIn { get; set; }
		public bool PlusOneAllowed { get; set; }

		public DateTime StartDate { get; set; }
		public DateTime EndDate { get; set; }
	}
}