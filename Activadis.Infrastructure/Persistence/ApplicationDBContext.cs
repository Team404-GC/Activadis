using Activadis.Infrastructure.Persistence.Seeders;
using Microsoft.EntityFrameworkCore;
using Activadis.Domain.Entities;
using Activadis.Infrastructure.Persistence.Configurations;

namespace Activadis.Infrastructure.Persistence
{
    public class ApplicationDBContext : DbContext
    {
        public DbSet<User> Users => Set<User>();
        public DbSet<SignUp> SignUps => Set<SignUp>();
        public DbSet<Activity> Activities => Set<Activity>();


        public ApplicationDBContext(DbContextOptions<ApplicationDBContext> options)
            : base(options) { }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<Activity>(options => options.Configure());
            builder.Entity<SignUp>(options => options.Configure());
            builder.Entity<User>(options => options.Configure());

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
