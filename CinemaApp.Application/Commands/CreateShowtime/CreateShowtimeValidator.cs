using FluentValidation;

namespace CinemaApp.Application.Commands.CreateShowtime
{
    public class CreateShowtimeValidator : AbstractValidator<CreateShowtimeRequest>
    {
        public CreateShowtimeValidator()
        {
            RuleFor(r => r.AuditoriumId).GreaterThan(0);
            RuleFor(r => r.SessionDate).NotNull();
            RuleFor(r => r.MovieId).NotEmpty().NotNull();
        }
    }
}
