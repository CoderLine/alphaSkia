namespace AlphaSkia;

/// <summary>
/// The image formats <see cref="AlphaSkiaImage.Decode"/> can decode.
/// </summary>
[Flags]
public enum AlphaSkiaImageFormats
{
    /// <summary>No image format.</summary>
    None = 0,
    /// <summary>Portable Network Graphics.</summary>
    Png = 1,
    /// <summary>Windows Bitmap.</summary>
    Bmp = 2,
    /// <summary>Graphics Interchange Format.</summary>
    Gif = 4,
    /// <summary>Windows Icon.</summary>
    Ico = 8,
    /// <summary>Wireless Bitmap.</summary>
    Wbmp = 16
}
