using Ardalis.Specification;
using CinemaApp.Core.Models;

namespace CinemaApp.Core.Specifications
{
    public sealed class GetAuditoriumByIdSpec : Specification<AuditoriumEntity>, ISingleResultSpecification<AuditoriumEntity>
    {
        public GetAuditoriumByIdSpec(int auditoriumId)
        {
            Query
                .Where(a => a.Id == auditoriumId)
                .Include(a => a.Seats);
        }
    }
}
