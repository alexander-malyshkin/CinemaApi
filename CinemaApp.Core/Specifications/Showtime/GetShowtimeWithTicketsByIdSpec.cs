using Ardalis.Specification;
using CinemaApp.Core.Models;

namespace CinemaApp.Core.Specifications.Showtime
{
    public sealed class GetShowtimeWithTicketsByIdSpec : Specification<ShowtimeEntity>, ISingleResultSpecification<ShowtimeEntity>
    {
        public GetShowtimeWithTicketsByIdSpec(int showtimeId)
        {
            Query
                .Where(showtime => showtime.Id == showtimeId)
                .Include(showtime => showtime.Tickets);
        }
    }
}
