using Activadis.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Activadis.Infrastructure;
using Activadis.Application;
using Microsoft.AspNetCore.Localization;
using System.Globalization;
using Activadis.API.Converters;

namespace Activadis.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.RegisterAPI(builder.Configuration);
            builder.Services.RegisterApplication();
            builder.Services.RegisterInfrastructure(builder.Configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("The connectionString was not found!"));

            builder.Services.AddControllers().AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.Converters.Add(new UtcDateTimeJsonConverter());
            });

            builder.Services.AddCors(options =>
            {
                options.AddDefaultPolicy(policy =>
                {
                    policy.WithOrigins("https://localhost:4200")
                        .AllowAnyHeader()
                        .AllowAnyMethod();
                });
            });

            var app = builder.Build();

            using (var scope = app.Services.CreateScope())
            {
                ApplicationDBContext context = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
                context.Database.Migrate();
            }

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger(options =>
                    options.RouteTemplate = "{documentName}/specifications.json");

                app.UseSwaggerUI(options =>
                {
                    options.RoutePrefix = "api";
                    options.SwaggerEndpoint("specifications.json", "API");
                });

                app.MapGet("/", context =>
                {
                    context.Response.Redirect("/api");
                    return Task.CompletedTask;
                });
            }

            app.UseRequestLocalization(new RequestLocalizationOptions()
            {
                DefaultRequestCulture = new RequestCulture(CultureInfo.InvariantCulture)
            });

            app.UseCors();

            app.UseHttpsRedirection();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseRateLimiter();
            app.UseExceptionHandler("/Error");
            app.MapControllers();

            app.Run();
        }
    }
}
