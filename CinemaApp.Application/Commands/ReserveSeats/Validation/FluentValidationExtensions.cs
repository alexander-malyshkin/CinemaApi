using CinemaApp.Core.Models;
using FluentValidation;

namespace CinemaApp.Application.Commands.ReserveSeats.Validation
{
    public static class FluentValidationExtensions
    {
        public static IRuleBuilderOptions<ReserveSeatsRequest, ICollection<RowSeat>> ShouldBeContiguous(
            this IRuleBuilder<ReserveSeatsRequest, ICollection<RowSeat>> ruleBuilder)
        {
            return ruleBuilder.SetValidator(new ContiguousSeatsValidator());
        }
    }
}
