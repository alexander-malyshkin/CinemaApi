using CinemaApp.Application.Commands.BuySeats;
using CinemaApp.Application.Commands.Shared;
using CinemaApp.Application.Shared;
using CinemaApp.Core.Exceptions;
using CinemaApp.Core.Models;
using CinemaApp.Core.RepositoryContracts;
using CinemaApp.Utilities;
using FluentValidation;
using Microsoft.Extensions.Configuration;
using ValidationException = System.ComponentModel.DataAnnotations.ValidationException;

namespace CinemaApp.Application.Commands.ConfirmReservation
{
    public sealed class ConfirmReservationHandler : ReservationHandlerBase<ConfirmReservationRequest, ConfirmReservationResponse>
    {
        private readonly ITicketsRepository _ticketsRepository;

        private const string ReservationNotValidError = "Reservation is no longer valid and therefore cannot be confirmed";
        private const string TicketAlreadyPaidError = "Ticket has already been paid for";
        
        public ConfirmReservationHandler(IValidator<ConfirmReservationRequest> validator,
                                         ITicketsRepository ticketsRepository,
                                         IConfiguration configuration) 
            : base(validator, configuration)
        {
            _ticketsRepository = ticketsRepository;
        }
        protected override ConfirmReservationResponse ToResponse(ResponseBase errorResponse)
        {
            return new ConfirmReservationResponse(errorResponse.Success, errorResponse.Title, errorResponse.Details, errorResponse.RequestValid);
        }
        protected async override Task<ConfirmReservationResponse> HandleInternal(ConfirmReservationRequest request, CancellationToken ct)
        {
            TicketEntity? foundTicket = await _ticketsRepository.GetAsync(request.ReservationId!.Value, ct);
            if (foundTicket is null)
                throw new EntityNotFoundException(nameof(request.ReservationId), $"Reservation '{request.ReservationId}' not found");
            
            SemaphoreSlim showtimeLock = GetOrAddShowtimeLock(foundTicket.ShowtimeId);
            // this lock aims to make any other reservation or purchase of tickets impossible for the current showtime
            using (await showtimeLock.EnterAsync())
            {
                if (foundTicket.Paid)
                    throw new ValidationException(TicketAlreadyPaidError);
                
                if (!foundTicket.ReservationValid(ReservationValidityPeriodInMinutes))
                    throw new ValidationException(ReservationNotValidError);
                
                var confirmedTicket = await _ticketsRepository.ConfirmPaymentAsync(foundTicket, ct);
                return ConfirmReservationResponse.SuccessfulResponse;
            }
        }
    }
}
