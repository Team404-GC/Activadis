using Activadis.API.Converters;
using Activadis.Application.Interfaces;
using Activadis.Shared.DTOs;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using System.Collections;
using System.Reflection;
using System.Security.Authentication;
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
            services.RegisterControllers();

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
                    // A deactivated user keeps a valid token, so the status is checked on every request.
                    OnTokenValidated = async (context) =>
                    {
                        IAuthService authService = context.HttpContext.RequestServices.GetRequiredService<IAuthService>();

                        string? nameIdentifier = context.Principal?.FindFirstValue(ClaimTypes.NameIdentifier);
                        if (!Guid.TryParse(nameIdentifier, out Guid userId) || !await authService.IsActiveAsync(userId))
                            context.Fail(new AuthenticationException("Je account is niet actief."));
                    },

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

                        // An inactive account is rejected in OnTokenValidated, its message is shown instead of the default one.
                        string message = context.AuthenticateFailure is AuthenticationException exception
                            ? exception.Message
                            : "Je bent niet ingelogd.";

                        ApiResponse<object> response = ApiResponse<object>.Fail(message);
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

                options.OnRejected = async (context, cancellationToken) =>
                {
                    context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                    context.HttpContext.Response.ContentType = "application/json";

                    ApiResponse<object> response = ApiResponse<object>.Fail("De limiet voor het aantal verzoeken is overschreden. Probeer het later opnieuw.");
                    await context.HttpContext.Response.WriteAsJsonAsync(response, cancellationToken);
                };
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

        private static IServiceCollection RegisterControllers(this IServiceCollection services)
        {
            IMvcBuilder builder = services.AddControllers();
            
            builder.AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.Converters.Add(new UtcDateTimeJsonConverter());
            });

            builder.ConfigureApiBehaviorOptions(options =>
            {
                options.InvalidModelStateResponseFactory = (context) =>
                {
                    if (context.ActionDescriptor is not ControllerActionDescriptor controllerDescriptor)
                        return new LocalRedirectResult("/Error");

                    ParameterDescriptor? parameter = controllerDescriptor.Parameters.FirstOrDefault(p => p.BindingInfo?.BindingSource?.Id == "Body" || (p.ParameterType.IsClass && p.ParameterType != typeof(string)));
                    if (parameter is null)
                        return new LocalRedirectResult("/Error");

                    Type parameterType = parameter.ParameterType;
                    MethodInfo? method = typeof(ApiErrors).GetMethod("Build", BindingFlags.Public | BindingFlags.Static);
                    if (method is null)
                        return new LocalRedirectResult("/Error");

                    List<ApiErrors> errors = [..context.ModelState
                        .DistinctBy(x => x.Key)
                        .Where(x => x.Value is not null)
                        .Select(x =>
                        {
                            List<string> messages = [.. x.Value!.Errors.Select(e => e.ErrorMessage)];
                            return (ApiErrors) method.Invoke(null, [x.Key, messages])!;
                        })];

                    return new BadRequestObjectResult(
                        ApiResponse<object>.Error(errors)
                    );
                };
            });

            return services;
        }
    }
}
