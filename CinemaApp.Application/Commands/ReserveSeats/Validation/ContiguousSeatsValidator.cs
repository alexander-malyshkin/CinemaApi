using CinemaApp.Core.Models;
using FluentValidation;
using FluentValidation.Validators;

namespace CinemaApp.Application.Commands.ReserveSeats.Validation
{
    public class ContiguousSeatsValidator : PropertyValidator<ReserveSeatsRequest, ICollection<RowSeat>>, IPropertyValidator<ReserveSeatsRequest, ICollection<RowSeat>>
    {
        public override string Name => "ContiguousSeatsValidator";
        
        public override bool IsValid(ValidationContext<ReserveSeatsRequest> context, ICollection<RowSeat> seatsRequested)
        {
            if (seatsRequested is null || seatsRequested.Count == 0)
            {
                // if no seats are requested, then we cannot enforce the rule
                return true;
            }

            var seats = seatsRequested
                .OrderBy(s => s.Row)
                .ThenBy(s => s.SeatNumber)
                .ToArray();
            
            var previousSeat = seats.First();
            foreach (RowSeat seat in seats.Skip(1))
            {
                // we don't enforce contiguous seats across rows
                if (seat.Row != previousSeat.Row)
                {
                    previousSeat = seat;
                    continue;
                }

                // if the seat number is not adjacent to the previous seat, then the seats are not contiguous
                if (seat.SeatNumber != previousSeat.SeatNumber + 1)
                    return false;

                previousSeat = seat;
            }

            return true;
        }
        
        protected override string GetDefaultMessageTemplate(string errorCode)
        {
            return "Only contiguous seats may be requested";
        }
    }
}
