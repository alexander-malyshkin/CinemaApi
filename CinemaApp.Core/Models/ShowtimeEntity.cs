namespace CinemaApp.Core.Models
{
    public class ShowtimeEntity
    {
        public int Id { get; set; }
        public virtual MovieEntity Movie { get; set; }
        public DateTime SessionDate { get; set; }
        public int AuditoriumId { get; set; }
        public virtual ICollection<TicketEntity> Tickets { get; set; }

        public bool CanReserveSeats(ICollection<RowSeat> requestedSeats, int validPeriodInMinutes)
        {
            if (Tickets is null)
                throw new NotSupportedException("Tickets collection is not loaded");
            
            SeatEntity[] reservedOrBoughtSeats = Tickets
                .Where(t => t.ReservationValid(validPeriodInMinutes) || t.Paid)
                .SelectMany(t => t.Seats)
                .ToArray();
            
            return requestedSeats
                .All(requestedSeat => !reservedOrBoughtSeats.Any(reservedSeat => 
                    reservedSeat.Row == requestedSeat.Row && reservedSeat.SeatNumber == requestedSeat.SeatNumber)
                );
        }
    }
}
