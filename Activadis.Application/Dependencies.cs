using Microsoft.Extensions.DependencyInjection;
using Activadis.Application.Interfaces;
using Activadis.Application.Services;

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
            services.AddScoped<ITokenService, TokenService>();
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IActivityService, ActivityService>();
            services.AddScoped<ISignUpService, SignUpService>();

            services.AddScoped<IEloService, EloService>();
            services.AddScoped<EloCalculationService>();
            services.AddScoped<LeaderboardService>();

            return services;
        }
    }
}
