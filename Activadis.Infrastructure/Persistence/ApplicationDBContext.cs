using Activadis.Domain.Entities;
using Activadis.Infrastructure.Persistence.Seeders;
using Microsoft.EntityFrameworkCore;

namespace Activadis.Infrastructure.Persistence
{
    public class ApplicationDBContext(DbContextOptions<ApplicationDBContext> options) : DbContext(options)
    {
        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSeeding((context, _) =>
            {
                context.UseUserSeeder();
            });
            base.OnConfiguring(optionsBuilder);

        }
    }
}
