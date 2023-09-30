package alphaskia;

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
}
