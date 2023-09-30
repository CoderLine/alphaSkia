namespace AlphaSkia;

/// <summary>
/// Represents a typeface to draw text.
/// </summary>
public sealed class AlphaSkiaTypeface : AlphaSkiaNative
{
    private readonly AlphaSkiaData? _nativeData;

    private AlphaSkiaTypeface(IntPtr native, AlphaSkiaData? nativeData = null)
        : base(native, NativeMethods.alphaskia_typeface_free)
    {
        _nativeData = nativeData;
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing)
        {
            _nativeData?.Dispose();
        }
    }

    public static AlphaSkiaTypeface? Register(byte[] data)
    {
        var nativeData = NativeMethods.alphaskia_data_new_copy(data, (ulong)data.LongLength);
        var typeface = NativeMethods.alphaskia_typeface_register(nativeData);
        if (typeface == IntPtr.Zero)
        {
            NativeMethods.alphaskia_data_free(nativeData);
            return null;
        }

        return new AlphaSkiaTypeface(typeface, new AlphaSkiaData(nativeData));
    }

    public static AlphaSkiaTypeface? Create(string name, bool bold, bool italic)
    {
        var typeface =
            NativeMethods.alphaskia_typeface_make_from_name(name, bold ? (byte)1 : (byte)0, italic ? (byte)1 : (byte)0);
        if (typeface == IntPtr.Zero)
        {
            return null;
        }

        return new AlphaSkiaTypeface(typeface);
    }
}