using Activadis.Shared.DTOs;
using Azure;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using System.Security.Claims;
using System.Text;
using System.Threading.RateLimiting;

namespace Activadis.API
{
    public static class Dependencies
    {
        public static IServiceCollection RegisterAPI(this IServiceCollection services, IConfiguration configuration)
        {
            services.RegisterAuthentication(configuration);
            services.RegisterRateLimiter();
            services.RegisterSwagger();

            return services;
        }

        private static IServiceCollection RegisterAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
            string issuer = configuration.GetValue<string>("JWT:Issuer") ?? "";
            string key = configuration.GetValue<string>("JWT:Key") ?? "";

            AuthenticationBuilder builder = services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            });
            
            builder.AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters()
                {
                    ValidateIssuer = true,
                    ValidIssuer = issuer,

                    ValidateAudience = false,
                    ValidateLifetime = true,

                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),

                    ClockSkew = TimeSpan.Zero
                };

                options.Events = new JwtBearerEvents()
                {
                    OnForbidden = async (context) =>
                    {
                        context.Response.StatusCode = StatusCodes.Status403Forbidden;
                        context.Response.ContentType = "application/json";

                        ApiResponse<object> response = ApiResponse<object>.Fail("Je hebt geen toegang tot deze functie.");
                        await context.Response.WriteAsJsonAsync(response);
                    },

                    OnChallenge = async (context) =>
                    {
                        context.HandleResponse();

                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        context.Response.ContentType = "application/json";

                        ApiResponse<object> response = ApiResponse<object>.Fail("Je bent niet ingelogd.");
                        await context.Response.WriteAsJsonAsync(response);
                    }
                };
            });

            services.AddAuthorization();

            return services;
        }
        private static IServiceCollection RegisterRateLimiter(this IServiceCollection services)
        {
            services.AddRateLimiter(options =>
            {
                options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

                options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>((context) =>
                {
                    return RateLimitPartition.GetFixedWindowLimiter(
                        context.User.FindFirstValue(ClaimTypes.NameIdentifier) ?? context.Connection.RemoteIpAddress?.ToString() ?? "anonymous",
                        partition => new FixedWindowRateLimiterOptions()
                        {
                            PermitLimit = 50,
                            AutoReplenishment = true,
                            Window = TimeSpan.FromMinutes(1)
                        }
                    );
                });
            });

            return services;
        }

        private static IServiceCollection RegisterSwagger(this IServiceCollection services)
        {
            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("api", new OpenApiInfo
                {
                    Title = "Activadis API",
                    Version = null,
                });

                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    Description = "Enter your JWT token"
                });

                options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
                {
                    [new OpenApiSecuritySchemeReference("Bearer", document)] = []
                });
            });

            return services;
        }
    }
}
