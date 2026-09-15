using Activadis.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Activadis.Infrastructure.Persistence.Configurations
{
    public static class RatingConfiguration
    {
        public static void Configure(this EntityTypeBuilder<Rating> builder)
        {
            builder.HasOne(r => r.User)
                .WithMany()
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(r => r.Category)
                .WithMany(c => c.Ratings)
                .HasForeignKey(r => r.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(r => new { r.UserId, r.CategoryId })
                .IsUnique();

            builder.HasQueryFilter(r => r.DeletedAt == null);
        }
    }
}
