using Ardalis.Specification;
using CinemaApp.Core.Models;

namespace CinemaApp.Core.Specifications
{
    public sealed class GetTicketsEnrichedSpec : Specification<TicketEntity>
    {
        public GetTicketsEnrichedSpec(int showtimeId)
        {
            Query
                .Where(t => t.ShowtimeId == showtimeId)
                .Include(t => t.Showtime)
                .Include(t => t.Seats);
        }
    }
}
