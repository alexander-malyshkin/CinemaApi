namespace CinemaApp.Core.Models
{
    public class AuditoriumEntity
    {
        public int Id { get; set; }
        public virtual List<ShowtimeEntity> Showtimes { get; set; }
        public virtual ICollection<SeatEntity> Seats { get; set; }

        public bool SeatsValid(ICollection<RowSeat>? requestSeats)
        {
            if (requestSeats is null)
                return false;

            return requestSeats.All(requestSeat => 
                Seats.Any(seat => seat.Row == requestSeat.Row && seat.SeatNumber == requestSeat.SeatNumber)
            );
        }
    }
}
