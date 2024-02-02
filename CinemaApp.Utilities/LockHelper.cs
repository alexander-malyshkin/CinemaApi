namespace CinemaApp.Utilities;

public readonly struct LockHelper : IDisposable
{
    private readonly object? obj;
    public bool TimedOut { get; }

    private LockHelper(object obj)
    {
        this.obj = obj;
        TimedOut = false;
    }

    private LockHelper(bool timedOut)
    {
        obj = null;
        TimedOut = timedOut;
    }

    private static LockHelper LockTimedOut => new LockHelper(true);
        
    public static LockHelper Lock(object obj, TimeSpan timeout)
    {
        bool lockTaken = false;
        try
        {
            Monitor.TryEnter(obj, timeout, ref lockTaken);
            return lockTaken 
                ? new LockHelper(obj) 
                : LockHelper.LockTimedOut;
        }
        catch
        {
            if (lockTaken)
            {
                Monitor.Exit(obj);
            }

            throw;
        }
    }
        
    public void Dispose()
    {
        if (obj == null) return;
        Monitor.Exit(obj);
    }
}
