using Activadis.Infrastructure.Persistence.Seeders;
using Microsoft.EntityFrameworkCore;
using Activadis.Domain.Entities;

namespace Activadis.Infrastructure.Persistence
{
    public class ApplicationDBContext : DbContext
    {
        public DbSet<User> Users => Set<User>();

        public ApplicationDBContext(DbContextOptions<ApplicationDBContext> options)
            : base(options) { }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
        }

        protected override void OnConfiguring(DbContextOptionsBuilder builder)
        {
            builder.UseSeeding((context, _) =>
            {
                context.UseUserSeeder();
            });

            base.OnConfiguring(builder);

        }
    }
}
