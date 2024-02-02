using CinemaApp.Application.Commands.BuySeats;
using MediatR;

namespace CinemaApp.Application.Commands.ConfirmReservation
{
    public class ConfirmReservationRequest : IRequest<ConfirmReservationResponse>
    {
        public Guid? ReservationId { get; set; }
    }
}
