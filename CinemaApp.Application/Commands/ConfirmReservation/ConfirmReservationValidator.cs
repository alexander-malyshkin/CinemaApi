using FluentValidation;

namespace CinemaApp.Application.Commands.ConfirmReservation
{
    public class ConfirmReservationValidator : AbstractValidator<ConfirmReservationRequest>
    {
        public ConfirmReservationValidator()
        {
            RuleFor(r => r.ReservationId)
                .NotNull();
        }
    }
}
