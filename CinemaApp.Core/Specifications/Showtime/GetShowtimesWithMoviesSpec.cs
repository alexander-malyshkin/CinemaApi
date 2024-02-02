using System.Linq.Expressions;
using Ardalis.Specification;
using CinemaApp.Core.Models;

namespace CinemaApp.Core.Specifications.Showtime
{
    public sealed class GetShowtimesWithMoviesSpec : Specification<ShowtimeEntity>
    {
        public GetShowtimesWithMoviesSpec(Expression<Func<ShowtimeEntity, bool>>? predicate)
        {
            if (predicate is not null)
            {
                Query
                    .Where(predicate)
                    .Include(st => st.Movie);
            }
            else
            {
                Query
                    .Include(st => st.Movie);
            }
        }
    }
}
