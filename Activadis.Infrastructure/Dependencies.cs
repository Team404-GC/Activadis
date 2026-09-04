using Microsoft.Extensions.DependencyInjection;
using Activadis.Domain.Interfaces.Repositories;
using Activadis.Infrastructure.Repositories;
using Activadis.Infrastructure.Persistence;
using Activadis.Domain.Interfaces.Helpers;
using Activadis.Infrastructure.Helpers;
using Microsoft.EntityFrameworkCore;

namespace Activadis.Infrastructure
{
    public static class Dependencies
    {
        public static IServiceCollection RegisterInfrastructure(this IServiceCollection services, string connectionString)
        {
            services.RegisterPersistence(connectionString);
            services.RegisterRepositories();
            services.RegisterHelpers();

            return services;
        }

        private static IServiceCollection RegisterPersistence(this IServiceCollection services, string connectionString)
        {
            services.AddDbContext<ApplicationDBContext>(options =>
            {
                options.UseSqlServer(connectionString, sqlOptions =>
                {
                    sqlOptions.MigrationsHistoryTable("Migrations");
                });
            });

            return services;
        }

        private static IServiceCollection RegisterRepositories(this IServiceCollection services)
        {
            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
            services.AddScoped<IUserRepository, UserRepository>();

            return services;
        }

        private static IServiceCollection RegisterHelpers(this IServiceCollection services)
        {
            services.AddScoped<IPassword, Password>();

            return services;
        }
    }
}
