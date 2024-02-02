namespace CinemaApp.Core.ServiceContracts
{
    public interface ICacheService<T>
    {
        Task<T?> GetOrSet(string key, Func<string, CancellationToken, Task<T?>> factory, CancellationToken ct);
    }
}