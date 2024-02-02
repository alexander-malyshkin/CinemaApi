using FastEndpoints.Swagger;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace CinemaApp.Api
{
    public static class SwaggerExtensions
    {
        public static IServiceCollection AddSwaggerStartup(this IServiceCollection services)
        {
            services.SwaggerDocument(c =>
            {
                c.DocumentSettings = s =>
                {
                    s.Title = "CinemaApp.Api";
                    s.Version = "v1";
                };
            });

            return services;
        }

        public static IApplicationBuilder UseSwaggerStartup(this IApplicationBuilder app)
        {
            app.UseOpenApi();
            app.UseSwaggerUi(s => s.ConfigureDefaults());

            return app;
        }
    }
}
