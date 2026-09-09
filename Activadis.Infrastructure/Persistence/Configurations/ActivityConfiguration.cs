using Activadis.Domain.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Activadis.Infrastructure.Persistence.Configurations
{
    public static class ActivityConfiguration
    {
        public static void Configure(this EntityTypeBuilder<Activity> builder)
        {
            builder.Property(a => a.Name)
                    .IsRequired(true);

            builder.Property(a => a.Description)
                .IsRequired(true);

            builder.Property(a => a.Location)
                .IsRequired(true);
        }
    }
}
