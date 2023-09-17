namespace AlphaSkia;

/// <summary>
/// The base class for AlphaSkia objects wrapping native Skia objects.
/// </summary>
public class AlphaSkiaNative : IDisposable
{
    private readonly Action<IntPtr> _release;

    internal IntPtr Native { get; private set; }

    internal AlphaSkiaNative(IntPtr native, Action<IntPtr> release)
    {
        _release = release;
        Native = native;
    }

    private void ReleaseUnmanagedResources()
    {
        _release(Native);
        Native = IntPtr.Zero;
    }

    protected virtual void Dispose(bool disposing)
    {
        ReleaseUnmanagedResources();
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    ~AlphaSkiaNative()
    {
        Dispose(false);
    }
    
    protected void CheckDisposed()
    {
        if (Native == IntPtr.Zero)
        {
            throw new ObjectDisposedException("this");
        }
    }
}