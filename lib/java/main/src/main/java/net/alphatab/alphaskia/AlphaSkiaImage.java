package net.alphatab.alphaskia;

/**
 * Represents a final rendered image.
 */
public class AlphaSkiaImage extends AlphaSkiaNative {
    /**
     * Gets the width of the image.
     *
     * @return the width of the image
     */
    public native int getWidth();

    /**
     * Gets the height of the image.
     *
     * @return the height of the image
     */
    public native int getHeight();

    AlphaSkiaImage(long handle) {
        super(handle);
    }

    @Override
    public native void close();

    /**
     * Reads the raw pixel data of this image as byte array.
     *
     * @return A copy of the raw pixel data.
     */
    public native byte[] readPixels();

    /**
     * Encodes the image to a PNG.
     *
     * @return The raw PNG bytes for further usage.
     */
    public native byte[] toPng();

    /**
     * Decodes the given bytes into an image using supported image formats like PNG.
     * @param bytes The raw iamge bytes.
     * @return The decoded image or <code>null</code> if the image could not be decoded.
     */
    public static AlphaSkiaImage decode(byte[] bytes)
    {
        long handle = allocateDecoded(bytes);
        return handle == 0 ? null : new AlphaSkiaImage(handle);
    }

    private static native long allocateDecoded(byte[] bytes);


    /**
     * Creates an image from the raw pixels assuming the default internal pixel format of alphaSkia.
     * @param width The width of the image in pixels.
     * @param height The height of the image in pixels.
     * @param pixels The raw pixel bytes.
     * @return The decoded image or <code>null</code> if the creation of the image from the pixels failed.
     */
    public static AlphaSkiaImage fromPixels(int width, int height, byte[] pixels)
    {
        long handle = createFromPixels(width, height, pixels);
        return handle == 0 ? null : new AlphaSkiaImage(handle);
    }

    private static native long createFromPixels(int width, int height, byte[] pixels);
}
