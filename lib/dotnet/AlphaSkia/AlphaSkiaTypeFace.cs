namespace AlphaSkia;

/// <summary>
/// Represents a typeface to draw text.
/// </summary>
public sealed class AlphaSkiaTypeFace : AlphaSkiaNative
{
    private readonly AlphaSkiaData? _nativeData;

    private AlphaSkiaTypeFace(IntPtr native, AlphaSkiaData? nativeData = null)
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

    public static AlphaSkiaTypeFace? Register(byte[] data)
    {
        var nativeData = NativeMethods.alphaskia_data_new_copy(data, (ulong)data.LongLength);
        var typeface = NativeMethods.alphaskia_typeface_register(nativeData);
        if (typeface == IntPtr.Zero)
        {
            NativeMethods.alphaskia_data_free(nativeData);
            return null;
        }

        return new AlphaSkiaTypeFace(typeface, new AlphaSkiaData(nativeData));
    }

    public static AlphaSkiaTypeFace? Create(string name, bool bold, bool italic)
    {
        var typeface =
            NativeMethods.alphaskia_typeface_make_from_name(name, bold ? (byte)1 : (byte)0, italic ? (byte)1 : (byte)0);
        if (typeface == IntPtr.Zero)
        {
            return null;
        }

        return new AlphaSkiaTypeFace(typeface);
    }
}