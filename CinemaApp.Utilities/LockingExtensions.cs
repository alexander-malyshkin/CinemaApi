namespace CinemaApp.Utilities
{
    public static class LockingExtensions
    {
        public static async Task<IDisposable> EnterAsync(this SemaphoreSlim ss)
        {
            await ss.WaitAsync().ConfigureAwait (false);
            return Disposable.Create (() => ss.Release());
        }
    }
}
