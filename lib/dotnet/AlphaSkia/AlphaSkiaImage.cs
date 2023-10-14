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
            return NativeMethods.alphaskia_image_get_width(Handle);
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
            return NativeMethods.alphaskia_image_get_height(Handle);
        }
    }

    internal AlphaSkiaImage(IntPtr handle)
        : base(handle, NativeMethods.alphaskia_image_free)
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
        return NativeMethods.alphaskia_image_read_pixels(Handle, pixels, rowBytes) != 0;
    }

    /// <summary>
    /// Encodes the image to a PNG.
    /// </summary>
    /// <returns>The raw PNG bytes for further usage.</returns>
    public byte[]? ToPng()
    {
        CheckDisposed();
        var data = NativeMethods.alphaskia_image_encode_png(Handle);
        using var wrapper = new AlphaSkiaData(data);
        return wrapper.ToArray();
    }

    /// <summary>
    /// Decodes the given bytes into an image using supported image formats like PNG.  
    /// </summary>
    /// <param name="bytes">The raw iamge bytes.</param>
    /// <returns>The decoded image or <code>null</code> if the image could not be decoded.</returns>
    public static AlphaSkiaImage? Decode(byte[] bytes)
    {
        var image = NativeMethods.alphaskia_image_decode(bytes, (ulong)bytes.LongLength);
        return (image == IntPtr.Zero) ? null : new AlphaSkiaImage(image);
    }

    /// <summary>
    /// Creates an image from the raw pixels assuming the default internal pixel format of alphaSkia.
    /// </summary>
    /// <param name="width">The width of the image in pixels.</param>
    /// <param name="height">The height of the image in pixels.</param>
    /// <param name="pixels">The raw pixel bytes.</param>
    /// <returns>The decoded image or <code>null</code> if the creation of the image from the pixels failed.</returns>
    public static AlphaSkiaImage? FromPixels(int width, int height, byte[] pixels)
    {
        var image = NativeMethods.alphaskia_image_from_pixels(width, height, pixels);
        return image == IntPtr.Zero ? null : new AlphaSkiaImage(image);
    }
}