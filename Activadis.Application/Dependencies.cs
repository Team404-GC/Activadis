using Microsoft.Extensions.DependencyInjection;

namespace Activadis.Application
{
    public static class Dependencies
    {
        public static IServiceCollection RegisterApplication(this IServiceCollection services)
        {
            services.RegisterServices();

            return services;
        }

        private static IServiceCollection RegisterServices(this IServiceCollection services)
        {
            return services;
        }
    }
}
