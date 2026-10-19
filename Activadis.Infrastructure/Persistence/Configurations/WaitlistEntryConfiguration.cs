using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Activadis.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Activadis.Infrastructure.Persistence.Configurations
{
    public static class WaitlistEntryConfiguration
    {
        public static void Configure(this EntityTypeBuilder<WaitlistEntry> builder)
        {
            builder.Property(we => we.Email)
                .IsRequired();

            builder.HasIndex(we => new { we.Email, we.ActivityId, we.DeletedAt })
                .IsUnique();

            builder.HasOne(we => we.Activity)
                .WithMany(a => a.Waitlist)
                .HasForeignKey(we => we.ActivityId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
