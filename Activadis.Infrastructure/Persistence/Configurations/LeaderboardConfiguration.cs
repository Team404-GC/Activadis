using Activadis.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Activadis.Infrastructure.Persistence.Configurations
{
    public static class LeaderboardConfiguration
    {
        public static void Configure(this EntityTypeBuilder<Leaderboard> builder)
        {
            builder.Property(l => l.IsOverall)
                .IsRequired();

            builder.Property(l => l.Entries)
                .HasConversion(
                    v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                    v => System.Text.Json.JsonSerializer.Deserialize<List<LeaderboardEntry>>(v, (System.Text.Json.JsonSerializerOptions?)null) ?? new List<LeaderboardEntry>())
                .Metadata.SetValueComparer(new ValueComparer<List<LeaderboardEntry>>(
                    (c1, c2) => c1!.SequenceEqual(c2!),
                    c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                    c => c.ToList()));

            builder.HasIndex(l => new { l.CategoryId, l.IsOverall, l.CreatedAt })
                .IsDescending(false, false, true);

            builder.HasQueryFilter(l => l.DeletedAt == null);

            builder.HasOne(l => l.Category)
                .WithMany()
                .HasForeignKey(l => l.CategoryId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired(false);
        }
    }
}
