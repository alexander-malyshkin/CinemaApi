namespace CinemaApp.Utilities
{
    public class Disposable : IDisposable
    {
        private readonly Action _dispose;
        private bool _disposed = false;
        private Disposable(Action dispose)
        {
            _dispose = dispose ?? throw new ArgumentNullException(nameof(dispose));
        }
        public void Dispose()
        {
            if (!_disposed)
            {
                _dispose();
                _disposed = true;
            }
        }
        public static IDisposable Create(Action dispose)
        {
            return new Disposable(dispose);
        }
    }
}
