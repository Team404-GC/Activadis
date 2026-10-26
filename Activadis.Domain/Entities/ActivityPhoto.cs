using Activadis.Domain.Interfaces;

namespace Activadis.Domain.Entities
{
	public class ActivityPhoto : IEntity
	{
		public Guid Id { get; set; }
		public byte[] Image { get; set; } = [];
		public string ImageContentType { get; set; } = string.Empty;
		public int SortOrder { get; set; }

		public DateTime CreatedAt { get; set; }
		public DateTime? UpdatedAt { get; set; }
		public DateTime? DeletedAt { get; set; }

		// Navigation Keys
		public Guid ActivityId { get; set; }

		// Navigation Properties
		public Activity Activity { get; set; } = null!;
	}
}