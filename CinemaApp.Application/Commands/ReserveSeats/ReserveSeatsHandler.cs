using CinemaApp.Application.Commands.Shared;
using CinemaApp.Application.Shared;
using CinemaApp.Core.Exceptions;
using CinemaApp.Core.Models;
using CinemaApp.Core.RepositoryContracts;
using CinemaApp.Utilities;
using FluentValidation;
using Microsoft.Extensions.Configuration;
using ValidationException = System.ComponentModel.DataAnnotations.ValidationException;

namespace CinemaApp.Application.Commands.ReserveSeats
{
    public class ReserveSeatsHandler : ReservationHandlerBase<ReserveSeatsRequest, ReserveSeatsResponse>
    {
        private readonly ITicketsRepository _ticketsRepository;
        private readonly IShowtimesRepository _showtimesRepository;
        private readonly IAuditoriumsRepository _auditoriumsRepository;
        
        
        private const string SeatsReservationValidationError = "Some of the requested seats are already reserved or paid for";
        private const string SeatsValidationError = "The auditorium does not have some of the requested seats";

        public ReserveSeatsHandler(IValidator<ReserveSeatsRequest> validator, 
                                   ITicketsRepository ticketsRepository, 
                                   IShowtimesRepository showtimesRepository,
                                   IAuditoriumsRepository auditoriumsRepository,
                                   IConfiguration configuration)
            : base(validator, configuration)
        {
            _ticketsRepository = ticketsRepository;
            _showtimesRepository = showtimesRepository;
            _auditoriumsRepository = auditoriumsRepository;
        }
        protected override ReserveSeatsResponse ToResponse(ResponseBase errorResponse)
        {
            return new ReserveSeatsResponse(false, errorResponse.Title, errorResponse.Details, errorResponse.RequestValid);
        }
        protected override async Task<ReserveSeatsResponse> HandleInternal(ReserveSeatsRequest request, CancellationToken ct)
        {
            SemaphoreSlim showtimeLock = GetOrAddShowtimeLock(request.ShowtimeId);
            
            // Locking only a single showtime for validating and making seats reservations
            // we are using SemaphoreSlim instead of lock to allow async/await
            using (await showtimeLock.EnterAsync())
            {
                ShowtimeEntity? showtime = await _showtimesRepository.GetWithTicketsByIdAsync(request.ShowtimeId, ct);
                if (showtime is null)
                    throw new EntityNotFoundException(nameof(request.ShowtimeId), $"Showtime {request.ShowtimeId} not found");

                if (!showtime.CanReserveSeats(request.Seats!, ReservationValidityPeriodInMinutes))
                    throw new ValidationException(SeatsReservationValidationError);
                
                var auditoriumId = showtime.AuditoriumId;

                IEnumerable<SeatEntity> selectedSeats = await LookupAndValidateSeatsAsync(auditoriumId, request.Seats, ct);
            
                TicketEntity createdTicket = await _ticketsRepository.CreateAsync(showtime, selectedSeats, ct);

                Guid reservationId = createdTicket.Id;
                int numberOfSeats = createdTicket.Seats.Count;
                string movieTitle = showtime.Movie.Title;
                return new ReserveSeatsResponse(reservationId, numberOfSeats, auditoriumId, movieTitle);
            }
        }
        private async Task<IEnumerable<SeatEntity>> LookupAndValidateSeatsAsync(int auditoriumId, ICollection<RowSeat>? requestSeats, 
                                                                                CancellationToken ct)
        {
            var auditorium = await _auditoriumsRepository.GetAsync(auditoriumId, ct);
            if (auditorium is null)
                throw new EntityNotFoundException(nameof(auditoriumId), $"Auditorium {auditoriumId} not found");
            
            if (!auditorium.SeatsValid(requestSeats))
                throw new ValidationException(SeatsValidationError);

            return auditorium.Seats
                .Where(s => requestSeats!.Any(rs =>
                    rs.Row == s.Row && rs.SeatNumber == s.SeatNumber));
        }
    }
}
