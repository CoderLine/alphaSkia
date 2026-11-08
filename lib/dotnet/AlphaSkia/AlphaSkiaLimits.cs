namespace AlphaSkia;

/// <summary>
/// Process-wide limits protecting against untrusted input forcing large allocations. Input exceeding a limit is
/// rejected like invalid input (e.g. decoding returns <code>null</code>, BeginRender throws). A numeric limit of 0
/// disables it.
/// </summary>
public static class AlphaSkiaLimits
{
    private const int MaxImageDimensionLimit = 0;
    private const int MaxImagePixelsLimit = 1;
    private const int MaxFontDataLengthLimit = 2;
    private const int MaxFontSizeLimit = 3;
    private const int MaxTextLengthLimit = 4;
    private const int ImageFormatsLimit = 5;

    /// <summary>
    /// The largest width or height in pixels of decoded images, images created from pixels and render surfaces
    /// (after the render scale). Default: 32767.
    /// </summary>
    public static ulong MaxImageDimension
    {
        get => NativeMethods.alphaskia_get_limit(MaxImageDimensionLimit);
        set => NativeMethods.alphaskia_set_limit(MaxImageDimensionLimit, value);
    }

    /// <summary>
    /// The largest width * height of decoded images, images created from pixels and render surfaces. Default: 2^28.
    /// </summary>
    public static ulong MaxImagePixels
    {
        get => NativeMethods.alphaskia_get_limit(MaxImagePixelsLimit);
        set => NativeMethods.alphaskia_set_limit(MaxImagePixelsLimit, value);
    }

    /// <summary>
    /// The largest font data in bytes which can be registered as typeface. Default: 50 MiB.
    /// </summary>
    public static ulong MaxFontDataLength
    {
        get => NativeMethods.alphaskia_get_limit(MaxFontDataLengthLimit);
        set => NativeMethods.alphaskia_set_limit(MaxFontDataLengthLimit, value);
    }

    /// <summary>
    /// The largest font size for drawing and measuring text. Default: 4096.
    /// </summary>
    public static ulong MaxFontSize
    {
        get => NativeMethods.alphaskia_get_limit(MaxFontSizeLimit);
        set => NativeMethods.alphaskia_set_limit(MaxFontSizeLimit, value);
    }

    /// <summary>
    /// The longest text in UTF-16 code units for drawing and measuring. Default: 0 (unlimited).
    /// </summary>
    public static ulong MaxTextLength
    {
        get => NativeMethods.alphaskia_get_limit(MaxTextLengthLimit);
        set => NativeMethods.alphaskia_set_limit(MaxTextLengthLimit, value);
    }

    /// <summary>
    /// The image formats <see cref="AlphaSkiaImage.Decode"/> accepts. Default: <see cref="AlphaSkiaImageFormats.Png"/>.
    /// </summary>
    public static AlphaSkiaImageFormats ImageFormats
    {
        get => (AlphaSkiaImageFormats)NativeMethods.alphaskia_get_limit(ImageFormatsLimit);
        set => NativeMethods.alphaskia_set_limit(ImageFormatsLimit, (ulong)value);
    }
}
