global using FastEndpoints;
using System.Text.Json;
using System.Text.Json.Serialization;
using CinemaApp.Infrastructure.Repositories;
using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CinemaApp.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            WebApplicationBuilder appBuilder = CreateHostBuilder(args);
            WebApplication webApplication = appBuilder.Build();
            Configure(webApplication, webApplication.Environment);
            webApplication.Run();
        }

        private static WebApplicationBuilder CreateHostBuilder(string[] args)
        {
            WebApplicationBuilder appBuilder = WebApplication.CreateBuilder(args);

            appBuilder.Logging.AddConsole();
            
            ConfigureServices(appBuilder.Services, appBuilder.Configuration);

            return appBuilder;
        }
        
        private static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddFastEndpoints();
            services.AddSwaggerStartup();

            services.AddMediatR(config =>
            {
                config.RegisterServicesFromAssemblies(typeof(CinemaApp.Application.Shared.ResponseBase).Assembly);
            });
            services.AddValidatorsFromAssembly(typeof(CinemaApp.Application.Shared.ResponseBase).Assembly);

            services.AddAuthentication();
            services.AddAuthorization();
            services.AddDalServices();
            services.AddIntegrationServices(configuration);
            services.AddCustomResiliencePipelines();
        }
        
        private static void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseMiddleware<WebRequestLoggingMiddleware>();
            if (!env.IsDevelopment())
            {
                app.UseHsts();
            }

            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseFastEndpoints(config =>
            {
                config.Serializer.Options.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                config.Serializer.Options.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
            });

            app.UseSwaggerStartup();
            
            InitializeData(app);
        }
        private static void InitializeData(IApplicationBuilder app)
        {
            using var serviceScope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope();
            var context = serviceScope.ServiceProvider.GetService<CinemaContext>();
            SampleData.Initialize(context!);
        }
    }
}
