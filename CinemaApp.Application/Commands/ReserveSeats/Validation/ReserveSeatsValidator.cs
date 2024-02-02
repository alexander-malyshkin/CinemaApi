using FluentValidation;

namespace CinemaApp.Application.Commands.ReserveSeats.Validation
{
    public class ReserveSeatsValidator : AbstractValidator<ReserveSeatsRequest>
    {
        public ReserveSeatsValidator()
        {
            RuleFor(r => r.Seats)
                .NotNull().NotEmpty()!
                .ShouldBeContiguous();
            
            RuleFor(r => r.ShowtimeId)
                .GreaterThan(0);
        }
    }
}
