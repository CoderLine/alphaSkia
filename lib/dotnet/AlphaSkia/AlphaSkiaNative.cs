using System.ComponentModel;

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
        if (Native == IntPtr.Zero)
        {
            throw new ObjectDisposedException("this");
        }
    }
}