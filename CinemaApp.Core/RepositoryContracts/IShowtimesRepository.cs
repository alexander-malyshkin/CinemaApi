using System.Linq.Expressions;
using CinemaApp.Core.Models;

namespace CinemaApp.Core.RepositoryContracts
{
    public interface IShowtimesRepository
    {
        Task<ShowtimeEntity> CreateShowtime(ShowtimeEntity showtime, CancellationToken cancel);
        Task<IEnumerable<ShowtimeEntity>> GetAllAsync(Expression<Func<ShowtimeEntity, bool>> filter, CancellationToken cancel);
        Task<ShowtimeEntity?> GetWithMoviesByIdAsync(int id, CancellationToken cancel);
        Task<ShowtimeEntity?> GetWithTicketsByIdAsync(int id, CancellationToken cancel);
    }
}