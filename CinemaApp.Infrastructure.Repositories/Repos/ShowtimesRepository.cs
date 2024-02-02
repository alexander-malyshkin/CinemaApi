using System.Linq.Expressions;
using Ardalis.Specification;
using CinemaApp.Core.Models;
using CinemaApp.Core.RepositoryContracts;
using CinemaApp.Core.Specifications.Showtime;
using Microsoft.EntityFrameworkCore;

namespace CinemaApp.Infrastructure.Repositories.Repos
{
    public class ShowtimesRepository : EfRepository<ShowtimeEntity>, IShowtimesRepository
    {
        public ShowtimesRepository(CinemaContext dbContext) : base(dbContext)
        {
        }
        public ShowtimesRepository(CinemaContext dbContext, ISpecificationEvaluator specificationEvaluator) 
            : base(dbContext, specificationEvaluator)
        {
        }
        
        public Task<ShowtimeEntity> CreateShowtime(ShowtimeEntity showtime, CancellationToken cancel)
        {
            return AddAsync(showtime, cancel);
        }
        public async Task<IEnumerable<ShowtimeEntity>> GetAllAsync(Expression<Func<ShowtimeEntity, bool>>? filter, CancellationToken cancel)
        {
            return await ApplySpecification(new GetShowtimesWithMoviesSpec(filter))
                .ToArrayAsync(cancel);
        }
        public Task<ShowtimeEntity?> GetWithMoviesByIdAsync(int id, CancellationToken cancel)
        {
            return GetSingleOrDefaultEntity(new GetShowtimeWithMoviesByIdSpec(id), cancel);
        }
        public Task<ShowtimeEntity?> GetWithTicketsByIdAsync(int id, CancellationToken cancel)
        {
            return GetSingleOrDefaultEntity(new GetShowtimeWithTicketsByIdSpec(id), cancel);
        }
    }
}
