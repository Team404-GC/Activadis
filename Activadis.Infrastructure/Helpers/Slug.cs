using Activadis.Infrastructure.Persistence.Interceptors;
using Activadis.Infrastructure.Extensions;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Activadis.Domain.Interfaces;

namespace Activadis.Infrastructure.Helpers
{
    public static partial class Slug
    {
        [GeneratedRegex(@"[^a-z0-9\s-]")]
        private static partial Regex NoLettersAndNumbers();

        [GeneratedRegex(@"\s+")]
        private static partial Regex NoDoubleSpaces();

        private static string RemoveAllNonLettersAndNumbers(this string text)
            => NoLettersAndNumbers().Replace(text, string.Empty);

        private static string RemoveAllDoubleSpaces(this string text)
            => NoDoubleSpaces().Replace(text, " ").Trim();

        private static string Generate(string text)
        {
            text = text.ToLowerInvariant();

            text.RemoveAllNonLettersAndNumbers();
            text.RemoveAllDoubleSpaces();

            return text.Replace(" ", "-");
        }

        public static async Task<string> GenerateUniqueAsync(DbContext context, IHasSlug entity, string text)
        {
            string slug = Generate(text);

            // The entity already has this slug, counting it would give itself a new number.
            if (entity.Slug == slug)
                return slug;

            int count = await context.CountAsync<IHasSlug>(entity.GetType(), x => x.Slug != null && EF.Functions.Like(x.Slug, $"{slug}%"));
            if (count <= 0)
                return slug;

            return $"{slug}-{count}";
        }

        public static void CreateSlugs<TEntity>(this DbContext context) where TEntity : class, IEntity, IHasSlug
            => Task.Run(async () => await context.CreateSlugsAsync<TEntity>()).GetAwaiter().GetResult();

        public static async Task CreateSlugsAsync<TEntity>(this DbContext context) where TEntity : class, IEntity, IHasSlug
        {
            DbSet<TEntity> set = context.Set<TEntity>();

            IEnumerable<TEntity> entities = [..set.Where(x => x.Slug == null)];
            foreach (TEntity entity in entities)
            {
                string? source = SlugInterceptor.GetSlugSource(entity);
                if (source is null)
                    continue;

                entity.Slug = await GenerateUniqueAsync(context, entity, source);
                context.SaveChanges();
            }
        }
    }
}
