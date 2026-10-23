using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Activadis.Infrastructure.Extensions
{
    public static class DbContextExtensions
    {
        public static async Task<int> CountAsync<TEntity>(this DbContext context, Type type, Expression<Func<TEntity, bool>>? expression = null)
        {
            if (context is null)
                throw new InvalidOperationException("De database is niet gevonden.");

            if (type is null)
                throw new InvalidOperationException("Het type is niet gevonden.");

            object? set = typeof(DbContext).GetMethods()
                .First(m => m.Name == nameof(DbContext.Set) && m.IsGenericMethod && m.GetParameters().Length == 0)
                .MakeGenericMethod(type)
                .Invoke(context, null)!;

            if (set is null)
                throw new InvalidOperationException($"De tabel van het type {type.Name} is niet gevonden.");

            if (expression is not null)
            {
                if (type != typeof(TEntity))
                {
                    set = typeof(Queryable).GetMethod(nameof(Queryable.Cast))!
                        .MakeGenericMethod(typeof(TEntity))
                        .Invoke(null, [set])!;
                }

                set = ((IQueryable<TEntity>)set).Where(expression);
            }

            // Ensure we look up CountAsync using typeof(TEntity) to completely align parameters
            return await (Task<int>)typeof(EntityFrameworkQueryableExtensions).GetMethods()
                .First(x => x.Name == nameof(EntityFrameworkQueryableExtensions.CountAsync) && x.GetParameters().Length == 2 && x.GetParameters()[1].ParameterType == typeof(CancellationToken))
                .MakeGenericMethod(expression is not null ? typeof(TEntity) : type)
                .Invoke(null, [set, default(CancellationToken)])!;
        }
    }
}
