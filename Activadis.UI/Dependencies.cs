using Microsoft.AspNetCore.Components.Authorization;
using Activadis.UI.Application.Interfaces;
using Activadis.UI.Application.Services;
using Activadis.UI.Authentication;

namespace Activadis.UI
{
    public static class Dependencies
    {
        public static IServiceCollection RegisterDependencies(this IServiceCollection services, IConfiguration configuration)
        {
            services.RegisterAuthentication();
            services.RegisterHttpClient(configuration);
            services.RegisterServices();

            return services;
        }

        private static IServiceCollection RegisterAuthentication(this IServiceCollection services)
        {
            services.AddSingleton<SessionStorageService>();
            services.AddSingleton<AuthStateProvider>();

            // AuthStateProvider overrides AuthenticationStateProvider
            services.AddSingleton<AuthenticationStateProvider>(options =>
                options.GetRequiredService<AuthStateProvider>());

            services.AddTransient<AuthTokenHandler>();
            services.AddAuthorizationCore();

            return services;
        }

        private static IServiceCollection RegisterHttpClient(this IServiceCollection services, IConfiguration configuration)
        {
            IHttpClientBuilder builder = services.AddHttpClient("", options =>
            {
                string baseUrl = configuration.GetValue<string>("ApiBaseUrl")
                    ?? throw new InvalidOperationException("The API base url was not found.");

                options.BaseAddress = new Uri(baseUrl);
            });

            builder.AddHttpMessageHandler<AuthTokenHandler>();

            return services;
        }

        private static IServiceCollection RegisterServices(this IServiceCollection services)
        {
            services.AddScoped<IHttpService, HttpService>();
            services.AddScoped<IAuthService, AuthService>();

            return services;
        }
    }
}
