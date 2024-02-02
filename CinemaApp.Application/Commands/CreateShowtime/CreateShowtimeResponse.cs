using CinemaApp.Application.Shared;

namespace CinemaApp.Application.Commands.CreateShowtime
{
    public class CreateShowtimeResponse : ResponseBase
    {
        public int ShowtimeId { get; }
        public CreateShowtimeResponse(int showtimeId, bool success, string? title = null, string? details = null, bool requestValid = true) 
            : base(success, title, details, requestValid)
        {
            ShowtimeId = showtimeId;
        }
    }
}
