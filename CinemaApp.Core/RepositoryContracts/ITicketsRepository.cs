using CinemaApp.Core.Models;

namespace CinemaApp.Core.RepositoryContracts
{
    public interface ITicketsRepository
    {
        Task<TicketEntity> ConfirmPaymentAsync(TicketEntity ticketEntity, CancellationToken cancel);
        Task<TicketEntity> CreateAsync(ShowtimeEntity showtime, IEnumerable<SeatEntity> selectedSeats, CancellationToken cancel);
        Task<TicketEntity?> GetAsync(Guid id, CancellationToken cancel);
        Task<IEnumerable<TicketEntity>> GetEnrichedAsync(int showtimeId, CancellationToken cancel);
    }
}