namespace AlphaSkia;

/// <summary>
/// Represents a final rendered image. 
/// </summary>
public sealed class AlphaSkiaImage : AlphaSkiaNative
{
    /// <summary>
    /// Gets the width of the image.
    /// </summary>
    public int Width
    {
        get
        {
            CheckDisposed();
            return NativeMethods.alphaskia_image_get_width(Native);
        }
    }

    /// <summary>
    /// Gets the height of the image.
    /// </summary>
    public int Height
    {
        get
        {
            CheckDisposed();
            return NativeMethods.alphaskia_image_get_height(Native);
        }
    }

    internal AlphaSkiaImage(IntPtr native)
        : base(native, NativeMethods.alphaskia_image_free)
    {
    }

    /// <summary>
    /// Reads the raw pixel data of this image into the given target.
    /// </summary>
    /// <param name="pixels">The target pointer to which to copy the pixel data.</param>
    /// <param name="rowBytes">The number of bytes per image row.</param>
    /// <returns><code>true</code> if the reading was successful, otherwise <code>false</code></returns>
    public bool ReadPixels(IntPtr pixels, ulong rowBytes)
    {
        CheckDisposed();
        return NativeMethods.alphaskia_image_read_pixels(Native, pixels, rowBytes) != 0;
    }

    /// <summary>
    /// Encodes the image to a PNG.
    /// </summary>
    /// <returns>The raw PNG bytes for further usage.</returns>
    public byte[]? ToPng()
    {
        CheckDisposed();
        var data = NativeMethods.alphaskia_image_encode_png(Native);
        using var wrapper = new AlphaSkiaData(data);
        return wrapper.ToArray();
    }
}