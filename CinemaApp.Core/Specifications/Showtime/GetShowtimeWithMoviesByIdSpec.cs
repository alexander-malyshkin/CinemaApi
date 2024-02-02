using Ardalis.Specification;
using CinemaApp.Core.Models;

namespace CinemaApp.Core.Specifications.Showtime
{
    public sealed class GetShowtimeWithMoviesByIdSpec : Specification<ShowtimeEntity>, ISingleResultSpecification<ShowtimeEntity>
    {
        public GetShowtimeWithMoviesByIdSpec(int showtimeId)
        {
            Query
                .Where(st => st.Id == showtimeId)
                .Include(st => st.Movie);
        }
    }
}
