using Microsoft.Extensions.DependencyInjection;
using Activadis.Application.Interfaces;
using Activadis.Application.Services;
using Activadis.Shared.DTOs.SignUp;
using Activadis.Application.Services.Confirmations;

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
            services.AddScoped<IEmailService, EmailService>();
            services.AddScoped<IUserService, UserService>();

            services.AddScoped<IEloService, EloService>();
            services.AddScoped<EloCalculationService>();
            services.AddScoped<LeaderboardService>();

            services.AddSingleton<IConfirmationService<SignUpRequest>, SignUpConfirmationService>();
            services.AddSingleton<IConfirmationService<SignOutRequest>, SignOutConfirmationService>();

            return services;
        }
    }
}
