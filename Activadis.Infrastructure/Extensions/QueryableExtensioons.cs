using Activadis.Domain.Interfaces;

namespace Activadis.Infrastructure.Extensions
{
    public static class QueryableExtensions
    {
        public static IQueryable<TEntity> WithIdentifier<TEntity>(this IQueryable<TEntity> source, string identifier) where TEntity : class, IEntity, IHasSlug
            => Guid.TryParse(identifier, out Guid id)
                ? source.Where(x => x.Id == id)
                : source.Where(x => x.Slug == identifier);
    }
}
