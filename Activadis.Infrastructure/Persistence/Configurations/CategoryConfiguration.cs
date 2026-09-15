using Activadis.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

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

            builder.HasIndex(c => c.Name)
                .IsUnique();

            builder.HasQueryFilter(c => c.DeletedAt == null);
        }
    }
}