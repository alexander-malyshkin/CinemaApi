using CinemaApp.Core.DTO;
using CinemaApp.Core.ServiceContracts;
using Microsoft.Extensions.DependencyInjection;

namespace CinemaApp.Infrastructure.Integrations.MovieProviders
{
    public class CachedMoviesProvider : IMoviesProvider
    {
        private readonly IMoviesProvider _resilientProvider;
        private readonly ICacheService<MovieDto> _singleMovieCache;
        private readonly ICacheService<ICollection<MovieDto>> _multipleMovieCache;

        public CachedMoviesProvider([FromKeyedServices(MovieApiConstants.MovieResilientProviderKey)] IMoviesProvider resilientProvider, 
                                    ICacheService<MovieDto> singleMovieCache,
                                    ICacheService<ICollection<MovieDto>> multipleMovieCache)
        {
            _resilientProvider = resilientProvider;
            _singleMovieCache = singleMovieCache;
            _multipleMovieCache = multipleMovieCache;
        }
        
        public Task<MovieDto?> GetMovieById(string movieId, CancellationToken ct)
        {
            return _singleMovieCache.GetOrSet(movieId, 
                async (key, ctToken) => await _resilientProvider.GetMovieById(key, ctToken), 
                ct);
        }
        
        public Task<ICollection<MovieDto>> SearchMovies(string search, CancellationToken ct)
        {
            return _multipleMovieCache.GetOrSet(search, 
                async (key, ctToken) => await _resilientProvider.SearchMovies(key, ctToken), 
                ct)!;
        }
    }
}
