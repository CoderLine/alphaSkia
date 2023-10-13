using System.Runtime.InteropServices;

namespace AlphaSkia;

internal sealed class AlphaSkiaData : AlphaSkiaNative
{
    public AlphaSkiaData(IntPtr native)
        : base(native, NativeMethods.alphaskia_data_free)
    {
    }

    public byte[] ToArray()
    {
        CheckDisposed();
        var dataLength = NativeMethods.alphaskia_data_get_length(Native);
        var data = new byte[dataLength];
        var dataPtr = NativeMethods.alphaskia_data_get_data(Native);
        Marshal.Copy(dataPtr, data, 0, data.Length);
        return data;
    }
}