using Ardalis.Specification;
using CinemaApp.Core.Models;
using CinemaApp.Core.RepositoryContracts;
using CinemaApp.Core.Specifications;
using Microsoft.EntityFrameworkCore;

namespace CinemaApp.Infrastructure.Repositories.Repos
{
    public class TicketsRepository : EfRepository<TicketEntity>, ITicketsRepository
    {
        public TicketsRepository(CinemaContext dbContext) : base(dbContext)
        {
        }
        public TicketsRepository(CinemaContext dbContext, ISpecificationEvaluator specificationEvaluator) 
            : base(dbContext, specificationEvaluator)
        {
        }
        public async Task<TicketEntity> ConfirmPaymentAsync(TicketEntity ticket, CancellationToken cancel)
        {
            ticket.Paid = true;
            await UpdateAsync(ticket, cancel);
            return ticket;
        }
        public async Task<TicketEntity> CreateAsync(ShowtimeEntity showtime, IEnumerable<SeatEntity> selectedSeats, CancellationToken cancel)
        {
            var newTicket = new TicketEntity
            {
                ShowtimeId = showtime.Id,
                Seats = selectedSeats.ToList()
            };

            await AddAsync(newTicket, cancel);
            
            return newTicket;
        }
        public Task<TicketEntity?> GetAsync(Guid id, CancellationToken cancel)
        {
            return GetByIdAsync(id, cancel);
        }
        public async Task<IEnumerable<TicketEntity>> GetEnrichedAsync(int showtimeId, CancellationToken cancel)
        {
            return await ApplySpecification(new GetTicketsEnrichedSpec(showtimeId))
                .ToArrayAsync(cancel);
        }
    }
}
