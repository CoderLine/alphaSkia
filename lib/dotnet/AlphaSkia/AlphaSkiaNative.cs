using System.ComponentModel;

namespace AlphaSkia;

/// <summary>
/// The base class for AlphaSkia objects wrapping native Skia objects.
/// </summary>
public class AlphaSkiaNative : IDisposable
{
    private readonly Action<IntPtr> _release;

    internal IntPtr Handle { get; private set; }

    internal AlphaSkiaNative(IntPtr handle, Action<IntPtr> release)
    {
        _release = release;
        Handle = handle;
    }

    private void ReleaseUnmanagedResources()
    {
        _release(Handle);
        Handle = IntPtr.Zero;
    }

    /// <summary>
    /// Releases the unmanaged resources used by this instnace and optionally releases the managed resources.
    /// </summary>
    /// <param name="disposing">
    /// <code>true</code> to release both managed and unmanaged resources; <code>false</code> to release only unmanaged resources.
    /// </param>
    protected virtual void Dispose(bool disposing)
    {
        ReleaseUnmanagedResources();
    }

    /// <inheritdoc />
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Releases unmanaged resources and performs other cleanup operations before this instance is reclaimed by garbage collection.
    /// </summary>
    ~AlphaSkiaNative()
    {
        Dispose(false);
    }

    /// <summary>
    /// Checks whether the object has already been disposed and throws an exception if so.
    /// </summary>
    /// <exception cref="ObjectDisposedException">Thrown if this instance was already disposed.</exception>
    protected void CheckDisposed()
    {
        if (Handle == IntPtr.Zero)
        {
            throw new ObjectDisposedException("this");
        }
    }
}