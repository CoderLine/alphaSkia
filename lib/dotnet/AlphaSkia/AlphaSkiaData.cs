using System.Runtime.InteropServices;

namespace AlphaSkia;

internal sealed class AlphaSkiaData : AlphaSkiaNative
{
    public AlphaSkiaData(IntPtr handle)
        : base(handle, NativeMethods.alphaskia_data_free)
    {
    }

    public AlphaSkiaData(byte[] data)
        : base(NativeMethods.alphaskia_data_new_copy(data, (ulong)data.LongLength), NativeMethods.alphaskia_data_free)
    {
    }

    public byte[] ToArray()
    {
        CheckDisposed();
        var dataLength = NativeMethods.alphaskia_data_get_length(Handle);
        var data = new byte[dataLength];
        var dataPtr = NativeMethods.alphaskia_data_get_data(Handle);
        Marshal.Copy(dataPtr, data, 0, data.Length);
        return data;
    }
}