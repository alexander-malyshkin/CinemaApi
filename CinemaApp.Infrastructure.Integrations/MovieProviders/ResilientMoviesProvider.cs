using CinemaApp.Core.DTO;
using CinemaApp.Core.ServiceContracts;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Registry;

namespace CinemaApp.Infrastructure.Integrations.MovieProviders
{
    public class ResilientMoviesProvider : IMoviesProvider
    {
        private readonly IMoviesProvider _faultyProvider;
        private readonly ResiliencePipeline<MovieDto> _singleMovieResiliencePipeline;
        private readonly ResiliencePipeline<ICollection<MovieDto>> _multipleMovieResiliencePipeline;

        public ResilientMoviesProvider([FromKeyedServices(MovieApiConstants.MovieFaultyProviderKey)] IMoviesProvider faultyProvider,
                                       ResiliencePipelineProvider<string> resiliencePipelineProvider)
        {
            _faultyProvider = faultyProvider;
            _singleMovieResiliencePipeline = resiliencePipelineProvider.GetPipeline<MovieDto>(MovieApiConstants.SingleMovieResiliencePipelineKey);
            _multipleMovieResiliencePipeline = resiliencePipelineProvider.GetPipeline<ICollection<MovieDto>>(MovieApiConstants.MultipleMoviesResiliencePipelineKey);
        }
        
        public async Task<MovieDto?> GetMovieById(string movieId, CancellationToken ct)
        {
            return await _singleMovieResiliencePipeline.ExecuteAsync(async ctToken => await _faultyProvider.GetMovieById(movieId, ctToken), ct);
        }
        
        public async Task<ICollection<MovieDto>> SearchMovies(string search, CancellationToken ct)
        {
            return await _multipleMovieResiliencePipeline.ExecuteAsync(async ctToken => await _faultyProvider.SearchMovies(search, ctToken), ct);
        }
    }
}
