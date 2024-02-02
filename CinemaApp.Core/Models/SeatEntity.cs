namespace CinemaApp.Core.Models
{
    public class SeatEntity
    {
        public short Row { get; set; }
        public short SeatNumber { get; set; }
        public int AuditoriumId { get; set; }
        public virtual AuditoriumEntity Auditorium { get; set; }
    }
}
