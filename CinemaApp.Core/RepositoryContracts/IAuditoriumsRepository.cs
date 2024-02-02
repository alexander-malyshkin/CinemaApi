using CinemaApp.Core.Models;
namespace CinemaApp.Core.RepositoryContracts
{
    public interface IAuditoriumsRepository
    {
        Task<AuditoriumEntity?> GetAsync(int auditoriumId, CancellationToken cancel);
    }
}
