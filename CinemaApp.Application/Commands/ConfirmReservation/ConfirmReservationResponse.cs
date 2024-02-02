using CinemaApp.Application.Shared;

namespace CinemaApp.Application.Commands.BuySeats
{
    public class ConfirmReservationResponse : ResponseBase
    {
        public static ConfirmReservationResponse SuccessfulResponse => new ConfirmReservationResponse(true, null, null, true);
        public ConfirmReservationResponse(bool success, string? title, string? details, bool requestValid) : base(success, title, details, requestValid)
        {
        }
    }
}
