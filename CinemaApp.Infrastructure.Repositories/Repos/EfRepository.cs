using Ardalis.Specification;
using Ardalis.Specification.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CinemaApp.Infrastructure.Repositories.Repos
{
    public abstract class EfRepository<T> : RepositoryBase<T> where T : class
    {
        protected EfRepository(CinemaContext dbContext) : base(dbContext)
        {
        }
        protected EfRepository(CinemaContext dbContext, ISpecificationEvaluator specificationEvaluator) : base(dbContext, specificationEvaluator)
        {
        }

        protected Task<T?> GetSingleOrDefaultEntity(ISpecification<T> spec, CancellationToken ct)
        {
            return ApplySpecification(spec).FirstOrDefaultAsync(ct);
        }
    }
}
