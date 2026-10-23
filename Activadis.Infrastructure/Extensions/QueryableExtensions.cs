using Microsoft.EntityFrameworkCore;
using Activadis.Domain.Interfaces;

namespace Activadis.Infrastructure.Extensions
{
    public static class QueryableExtensions
    {
        public static IQueryable<TEntity> WithIdentifier<TEntity>(this IQueryable<TEntity> source, string identifier) where TEntity : class, IEntity
        {
            if (Guid.TryParse(identifier, out Guid id))
                return source.Where(x => x.Id == id);

            if (!typeof(IHasSlug).IsAssignableFrom(typeof(TEntity)))
                return source.Where(x => EF.Property<string?>(x, "Slug") == identifier);

            return source.Cast<IHasSlug>()
                .Where(x => x.Slug == identifier)
                .Cast<TEntity>();
        }
    }
}
