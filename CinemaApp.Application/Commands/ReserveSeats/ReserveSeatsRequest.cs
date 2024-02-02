using CinemaApp.Core.Models;
using MediatR;

namespace CinemaApp.Application.Commands.ReserveSeats
{
    public class ReserveSeatsRequest : IRequest<ReserveSeatsResponse>
    {
        public int ShowtimeId { get; set; }
        public ICollection<RowSeat>? Seats { get; set; }
    }
}
