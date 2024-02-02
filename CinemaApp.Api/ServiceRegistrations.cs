using System;
using System.Collections.Generic;
using System.Text.Json;
using CinemaApp.Core.DTO;
using CinemaApp.Core.RepositoryContracts;
using CinemaApp.Core.ServiceContracts;
using CinemaApp.Infrastructure.Integrations;
using CinemaApp.Infrastructure.Integrations.MovieProviders;
using CinemaApp.Infrastructure.Repositories;
using CinemaApp.Infrastructure.Repositories.Repos;
using CinemaApp.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using StackExchange.Redis;

namespace CinemaApp.Api
{
    public static class ServiceRegistrations
    {
        public static IServiceCollection AddIntegrationServices(this IServiceCollection services, IConfiguration configuration)
        {
            RegisterMoviesProvider(services);
            
            // Redis:
            services.AddSingleton<IConnectionMultiplexer>(sp =>
            {
                string redisOptions = configuration.GetValue<string>("CacheOptions:Redis")
                    ?? throw new ApplicationException("Redis connection string not found");
                return ConnectionMultiplexer.Connect(redisOptions);
            });

            services.AddSingleton(typeof(ICacheService<>), typeof(RedisService<>));
            services.AddSingleton(new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            return services;
        }
        

        public static IServiceCollection AddDalServices(this IServiceCollection services)
        {
            services.AddDbContext<CinemaContext>(options =>
            {
                options.UseInMemoryDatabase("CinemaDb")
                    .EnableSensitiveDataLogging()
                    .ConfigureWarnings(b => b.Ignore(InMemoryEventId.TransactionIgnoredWarning));
            }, ServiceLifetime.Singleton);
            
            services.AddSingleton<IShowtimesRepository, ShowtimesRepository>();
            services.AddSingleton<ITicketsRepository, TicketsRepository>();
            services.AddSingleton<IAuditoriumsRepository, AuditoriumsRepository>();
            return services;
        }

        public static IServiceCollection AddCustomResiliencePipelines(this IServiceCollection services)
        {
            services.AddResiliencePipeline<string, MovieDto>(
                MovieApiConstants.SingleMovieResiliencePipelineKey, 
                (builder, context) =>
            {
                ResiliencePipelines.AddHedgingRetryWithCircuitBreakerPipeline(builder);
            });
            services.AddResiliencePipeline<string, ICollection<MovieDto>>(
                MovieApiConstants.MultipleMoviesResiliencePipelineKey, 
                (builder, context) =>
                {
                    ResiliencePipelines.AddHedgingRetryWithCircuitBreakerPipeline(builder);
                });
            
            return services;
        }

        private static IServiceCollection RegisterMoviesProvider(IServiceCollection services)
        {
            return services.RegisterDecorator<IMoviesProvider, FakeMoviesProvider, ResilientMoviesProvider, CachedMoviesProvider>(
                MovieApiConstants.MovieFaultyProviderKey, MovieApiConstants.MovieResilientProviderKey);
        }

        public static IServiceCollection RegisterDecorator<TContract, TUnderlying, TDecorator>(this IServiceCollection services, string key) 
            where TContract : class
            where TUnderlying : class, TContract
            where TDecorator : class, TContract
        {
            return services
                .AddKeyedSingleton<TContract, TUnderlying>(key)
                .AddSingleton<TContract, TDecorator>();
        }
        
        public static IServiceCollection RegisterDecorator<TContract, TUnderlying1, TUnderlying2, TDecorator>(
            this IServiceCollection services, string underlying1Key, string underlying2Key) 
            where TContract : class
            where TUnderlying1 : class, TContract
            where TUnderlying2 : class, TContract
            where TDecorator : class, TContract
        {
            return services
                .AddKeyedSingleton<TContract, TUnderlying1>(underlying1Key)
                .AddKeyedSingleton<TContract, TUnderlying2>(underlying2Key)
                .AddSingleton<TContract, TDecorator>();
        }
    }
}
