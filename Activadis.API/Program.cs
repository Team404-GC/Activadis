using Activadis.Infrastructure;

namespace Activadis.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.RegisterInfrastructure(builder.Configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("The connectionString was not found!"));

            builder.Services.AddControllers();
            builder.Services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("api", new Microsoft.OpenApi.OpenApiInfo()
                {
                    Title = "API",
                    Version = null
                });
            });

            var app = builder.Build();

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
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
