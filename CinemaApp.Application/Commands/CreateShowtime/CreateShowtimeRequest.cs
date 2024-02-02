using MediatR;

namespace CinemaApp.Application.Commands.CreateShowtime
{
    public class CreateShowtimeRequest : IRequest<CreateShowtimeResponse>
    {
        public int AuditoriumId { get; set; }

        public DateTime? SessionDate { get; set; }

        public string MovieId { get; set; }
    }
}
