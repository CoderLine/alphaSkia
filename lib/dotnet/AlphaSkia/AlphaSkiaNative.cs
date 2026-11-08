namespace AlphaSkia;

/// <summary>
/// The base class for AlphaSkia objects wrapping native Skia objects.
/// </summary>
/// <remarks>
/// Instances are not thread-safe. Disposing an object while another thread uses it is safe: the native object is
/// released once the running call returns, later calls throw <see cref="ObjectDisposedException"/>.
/// </remarks>
public class AlphaSkiaNative : IDisposable
{
    internal NativeHandle Handle { get; }

    internal AlphaSkiaNative(IntPtr handle, Action<IntPtr> release)
    {
        Handle = new NativeHandle(handle, release);
    }

    /// <summary>
    /// Releases the unmanaged resources used by this instnace and optionally releases the managed resources.
    /// </summary>
    /// <param name="disposing">
    /// <code>true</code> to release both managed and unmanaged resources; <code>false</code> to release only unmanaged resources.
    /// </param>
    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            Handle.Dispose();
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Checks whether the object has already been disposed and throws an exception if so.
    /// </summary>
    /// <exception cref="ObjectDisposedException">Thrown if this instance was already disposed.</exception>
    protected void CheckDisposed()
    {
        if (Handle.IsClosed || Handle.IsInvalid)
        {
            throw new ObjectDisposedException(GetType().Name);
        }
    }
}
