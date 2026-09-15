using Activadis.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace Activadis.Infrastructure.Persistence.Configurations
{
    public static class MatchConfiguration
    {
        public static void Configure(this EntityTypeBuilder<Match> builder)
        {
            builder.Property(m => m.Notes)
                .HasMaxLength(1000);

            builder.HasOne(m => m.Category)
                .WithMany(c => c.Matches)
                .HasForeignKey(m => m.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(m => m.CategoryId);
            builder.HasIndex(m => m.MatchDate);

            builder.HasQueryFilter(m => m.DeletedAt == null);
        }
    }
}