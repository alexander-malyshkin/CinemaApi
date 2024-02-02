using Ardalis.Specification;
using CinemaApp.Core.Models;
using CinemaApp.Core.RepositoryContracts;
using CinemaApp.Core.Specifications;

namespace CinemaApp.Infrastructure.Repositories.Repos
{
    public class AuditoriumsRepository : EfRepository<AuditoriumEntity>, IAuditoriumsRepository
    {
        public AuditoriumsRepository(CinemaContext dbContext) : base(dbContext)
        {
        }
        
        public AuditoriumsRepository(CinemaContext dbContext, ISpecificationEvaluator specificationEvaluator) 
            : base(dbContext, specificationEvaluator)
        {
        }
        
        public Task<AuditoriumEntity?> GetAsync(int auditoriumId, CancellationToken cancel)
        {
            return GetSingleOrDefaultEntity(new GetAuditoriumByIdSpec(auditoriumId), cancel);
        }
    }
}
