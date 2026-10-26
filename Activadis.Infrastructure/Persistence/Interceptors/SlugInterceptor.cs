using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Activadis.Infrastructure.Helpers;
using Microsoft.EntityFrameworkCore;
using System.Collections.Concurrent;
using Activadis.Domain.Interfaces;
using Activadis.Domain.Attributes;
using System.Reflection;

namespace Activadis.Infrastructure.Persistence.Interceptors
{
    public class SlugInterceptor : SaveChangesInterceptor
    {
        private static readonly ConcurrentDictionary<Type, PropertyInfo?> Properties = new ConcurrentDictionary<Type, PropertyInfo?>();

        public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
        {
            if (eventData.Context is not null)
                await SetSlugsAsync(eventData.Context);

            return await base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        private static async Task SetSlugsAsync(DbContext context)
        {
            IEnumerable<EntityEntry<IHasSlug>> entries = context.ChangeTracker.Entries<IHasSlug>()
                .Where(x => x.State == EntityState.Added || x.State == EntityState.Modified);

            foreach (EntityEntry<IHasSlug> entry in entries)
            {
                IHasSlug entity = entry.Entity;

                string? source = GetSlugSource(entity, entry);
                if (source is null)
                    continue;

                entity.Slug = await Slug.GenerateUniqueAsync(context, entity, source);
            }
        }

        public static string? GetSlugSource(IHasSlug entity, EntityEntry<IHasSlug>? entry = null)
        {
            Type type = entity.GetType();
            PropertyInfo? property = Properties.GetOrAdd(type, type
                => type.GetProperties().FirstOrDefault(property =>
                    property.GetCustomAttribute<SlugSourceAttribute>() is not null));

            if (property is null)
                return null;

            if (entry is not null && entry.State != EntityState.Added && !entry.Property(property.Name).IsModified)
                return null;

            string? value = property.GetValue(entity)?.ToString();
            if (string.IsNullOrWhiteSpace(value))
                return null;

            return value;
        }
    }
}
