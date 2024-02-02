using CinemaApp.Core.DTO;

namespace CinemaApp.Core.ServiceContracts
{
    public interface IMoviesProvider
    {
        Task<MovieDto?> GetMovieById(string movieId, CancellationToken ct);
        Task<ICollection<MovieDto>> SearchMovies(string search, CancellationToken ct);
    }
}