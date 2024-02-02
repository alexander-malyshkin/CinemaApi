using System.Collections.Concurrent;
using CinemaApp.Application.Shared;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Configuration;

namespace CinemaApp.Application.Commands.Shared
{
    public abstract class ReservationHandlerBase<TRequest, TResponse> : HandlerBase<TRequest, TResponse> 
        where TRequest : IRequest<TResponse>
        where TResponse : ResponseBase
    {
        private static readonly ConcurrentDictionary<int, SemaphoreSlim> ShowtimeLocks = new ConcurrentDictionary<int, SemaphoreSlim>();
        protected readonly int ReservationValidityPeriodInMinutes;
        protected ReservationHandlerBase(IValidator<TRequest> validator, IConfiguration configuration) : base(validator)
        {
            ReservationValidityPeriodInMinutes = int.TryParse(configuration["SeatReservation:ValidPeriodInMinutes"], out var res) 
                ? res
                : throw new NotSupportedException("ValidPeriodInMinutes is not set");
        }

        protected SemaphoreSlim GetOrAddShowtimeLock(int showtimeId)
        {
            return ShowtimeLocks.GetOrAdd(showtimeId, k => new SemaphoreSlim(1));
        }
    }
}
