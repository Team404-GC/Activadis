using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore;
using Activadis.Domain.Interfaces;

namespace Activadis.Infrastructure.Persistence.Configurations
{
    public static class IHasSlugConfiguration
    {
        public static void ConfigureSlugs(this ModelBuilder builder)
        {
            foreach (IMutableEntityType type in builder.Model.GetEntityTypes())
            {
                if (type.ClrType is null || !typeof(IHasSlug).IsAssignableFrom(type.ClrType))
                    continue;

                builder.Entity(type.ClrType, options =>
                {
                    options.HasIndex(nameof(IHasSlug.Slug));
                });
            }
        }
    }
}
