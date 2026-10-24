using Activadis.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Activadis.Infrastructure.Persistence.Configurations
{
	public static class ActivityPhotoConfiguration
	{
		public static void Configure(this EntityTypeBuilder<ActivityPhoto> builder)
		{
			builder.Property(ap => ap.ImageContentType)
				.IsRequired(true);

			builder.HasIndex(ap => new { ap.ActivityId, ap.SortOrder });

			builder.HasOne(ap => ap.Activity)
				.WithMany(a => a.Photos)
				.HasForeignKey(ap => ap.ActivityId)
				.OnDelete(DeleteBehavior.Cascade);
		}
	}
}
