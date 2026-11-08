using System.Runtime.InteropServices;

namespace AlphaSkia;

/// <summary>
/// Owns a native alphaSkia object. Passed to native methods, the marshaller keeps the object alive for the duration
/// of the call, also when it is disposed or finalized concurrently.
/// </summary>
internal sealed class NativeHandle : SafeHandle
{
    private readonly Action<IntPtr> _release;

    public NativeHandle(IntPtr handle, Action<IntPtr> release)
        : base(IntPtr.Zero, true)
    {
        _release = release;
        SetHandle(handle);
    }

    public override bool IsInvalid => handle == IntPtr.Zero;

    protected override bool ReleaseHandle()
    {
        _release(handle);
        return true;
    }
}
