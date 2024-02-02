using CinemaApp.Application.Shared;

namespace CinemaApp.Application.Commands.ReserveSeats
{
    public class ReserveSeatsResponse : ResponseBase
    {
        public Guid? ReservationId { get; }
        public int? NumberOfSeats { get; }
        public int? AuditoriumId { get; }
        public string? MovieTitle { get; }

        public ReserveSeatsResponse(Guid reservationId, int numberOfSeats, int auditoriumId, string movieTitle)
        : base(true, null, null, true)
        {
            ReservationId = reservationId;
            NumberOfSeats = numberOfSeats;
            AuditoriumId = auditoriumId;
            MovieTitle = movieTitle;
        }
        
        public ReserveSeatsResponse(bool success, string? title, string? details, bool requestValid) 
            : base(success, title, details, requestValid)
        {
        }
    }
}
