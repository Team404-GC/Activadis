using Activadis.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Activadis.Infrastructure.Persistence.Configurations
{
    public static class CategoryConfiguration
    {
        public static void Configure(this EntityTypeBuilder<Category> builder)
        {
            builder.Property(c => c.Name)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(c => c.Description)
                .HasMaxLength(500);

            builder.Property(c => c.RankTitles)
                .HasConversion(
                    v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                    v => System.Text.Json.JsonSerializer.Deserialize<Dictionary<double, string>>(v, (System.Text.Json.JsonSerializerOptions?)null) ?? new Dictionary<double, string>())
                .Metadata.SetValueComparer(new ValueComparer<Dictionary<double, string>>(
                    (c1, c2) => c1!.SequenceEqual(c2!),
                    c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                    c => new Dictionary<double, string>(c)));

            builder.HasIndex(c => c.Name)
                .IsUnique();

            builder.HasQueryFilter(c => c.DeletedAt == null);
        }
    }
}
