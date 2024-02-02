using CinemaApp.Utilities;

namespace CinemaApp.Infrastructure.Integrations
{
    public static class MovieApiConstants
    {
        public const string MovieFaultyProviderKey = "MoviesFaultyProvider";
        public const string MovieResilientProviderKey = "MovieResilientProvider";
        
        private const string MovieDtoKey = "MovieDto";
        private const string MoviesCollectionDtoKey = "MoviesCollectionDto";
        
        public static string SingleMovieResiliencePipelineKey => $"{ResiliencePipelines.HedgingRetryWithCircuitBreaker}-{MovieDtoKey}";
        public static string MultipleMoviesResiliencePipelineKey => $"{ResiliencePipelines.HedgingRetryWithCircuitBreaker}-{MoviesCollectionDtoKey}";
    }
}
