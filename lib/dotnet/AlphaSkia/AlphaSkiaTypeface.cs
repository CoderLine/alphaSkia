namespace AlphaSkia;

/// <summary>
/// Represents a typeface to draw text.
/// </summary>
public sealed class AlphaSkiaTypeface : AlphaSkiaNative
{
    private readonly AlphaSkiaData? _nativeData;

    private AlphaSkiaTypeface(IntPtr handle, AlphaSkiaData? nativeData = null)
        : base(handle, NativeMethods.alphaskia_typeface_free)
    {
        _nativeData = nativeData;
    }

    /// <inheritdoc />
    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing)
        {
            _nativeData?.Dispose();
        }
    }

    /// <summary>
    /// Register a new custom font from the given binary data containing the data of a font compatible with Skia (e.g. TTF).
    /// </summary>
    /// <param name="data">The raw binary data of the font.</param>
    /// <returns>The loaded typeface to use for text rendering or <code>null</code> if the loading failed.</returns>
    public static AlphaSkiaTypeface? Register(byte[] data)
    {
        var nativeData = new AlphaSkiaData(data);
        var typeface = NativeMethods.alphaskia_typeface_register(nativeData.Handle);
        if (typeface == IntPtr.Zero)
        {
            nativeData.Dispose();
            return null;
        }

        return new AlphaSkiaTypeface(typeface, nativeData);
    }
    
    /// <summary>
    /// Creates a typeface using the provided information.
    /// </summary>
    /// <param name="name">The name of the typeface.</param>
    /// <param name="bold">Whether the bold version of the typeface should be loaded.</param>
    /// <param name="italic">Whether the italic version of the typeface should be loaded.</param>
    /// <returns>The typeface if it can be found in the already loaded fonts or the system fonts, otherwise <code>null</code>.</returns>
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