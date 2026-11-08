package alphaTab.alphaSkia;

import java.util.EnumSet;
import java.util.Objects;

/**
 * Process-wide limits protecting against untrusted input forcing large allocations. Input exceeding a limit is
 * rejected like invalid input (e.g. decoding returns {@code null}, beginRender throws). A numeric limit of 0 disables it.
 */
public final class AlphaSkiaLimits {
    private static final int MAX_IMAGE_DIMENSION = 0;
    private static final int MAX_IMAGE_PIXELS = 1;
    private static final int MAX_FONT_DATA_LENGTH = 2;
    private static final int MAX_FONT_SIZE = 3;
    private static final int MAX_TEXT_LENGTH = 4;
    private static final int IMAGE_FORMATS = 5;

    private AlphaSkiaLimits() {
    }

    /**
     * Gets the current limit.
     *
     * @return The largest width or height in pixels of decoded images, images created from pixels and render surfaces
     * (after the render scale). Default: 32767.
     */
    public static long getMaxImageDimension() {
        return getLimit(MAX_IMAGE_DIMENSION);
    }

    /**
     * Sets the limit.
     *
     * @param value The largest width or height in pixels, 0 disables the limit.
     */
    public static void setMaxImageDimension(long value) {
        setLimit(MAX_IMAGE_DIMENSION, checkValue(value));
    }

    /**
     * Gets the current limit.
     *
     * @return The largest width * height of decoded images, images created from pixels and render surfaces.
     * Default: 2^28.
     */
    public static long getMaxImagePixels() {
        return getLimit(MAX_IMAGE_PIXELS);
    }

    /**
     * Sets the limit.
     *
     * @param value The largest width * height, 0 disables the limit.
     */
    public static void setMaxImagePixels(long value) {
        setLimit(MAX_IMAGE_PIXELS, checkValue(value));
    }

    /**
     * Gets the current limit.
     *
     * @return The largest font data in bytes which can be registered as typeface. Default: 50 MiB.
     */
    public static long getMaxFontDataLength() {
        return getLimit(MAX_FONT_DATA_LENGTH);
    }

    /**
     * Sets the limit.
     *
     * @param value The largest font data in bytes, 0 disables the limit.
     */
    public static void setMaxFontDataLength(long value) {
        setLimit(MAX_FONT_DATA_LENGTH, checkValue(value));
    }

    /**
     * Gets the current limit.
     *
     * @return The largest font size for drawing and measuring text. Default: 4096.
     */
    public static long getMaxFontSize() {
        return getLimit(MAX_FONT_SIZE);
    }

    /**
     * Sets the limit.
     *
     * @param value The largest font size, 0 disables the limit.
     */
    public static void setMaxFontSize(long value) {
        setLimit(MAX_FONT_SIZE, checkValue(value));
    }

    /**
     * Gets the current limit.
     *
     * @return The longest text in UTF-16 code units for drawing and measuring. Default: 0 (unlimited).
     */
    public static long getMaxTextLength() {
        return getLimit(MAX_TEXT_LENGTH);
    }

    /**
     * Sets the limit.
     *
     * @param value The longest text in UTF-16 code units, 0 disables the limit.
     */
    public static void setMaxTextLength(long value) {
        setLimit(MAX_TEXT_LENGTH, checkValue(value));
    }

    /**
     * Gets the current limit.
     *
     * @return The image formats {@link AlphaSkiaImage#decode(byte[])} accepts. Default: PNG.
     */
    public static EnumSet<AlphaSkiaImageFormat> getImageFormats() {
        long mask = getLimit(IMAGE_FORMATS);
        var formats = EnumSet.noneOf(AlphaSkiaImageFormat.class);
        for (var format : AlphaSkiaImageFormat.values()) {
            if ((mask & format.getValue()) != 0) {
                formats.add(format);
            }
        }
        return formats;
    }

    /**
     * Sets the accepted image formats.
     *
     * @param formats The image formats {@link AlphaSkiaImage#decode(byte[])} accepts.
     */
    public static void setImageFormats(EnumSet<AlphaSkiaImageFormat> formats) {
        long mask = 0;
        for (var format : Objects.requireNonNull(formats, "formats")) {
            mask |= format.getValue();
        }
        setLimit(IMAGE_FORMATS, mask);
    }

    private static long checkValue(long value) {
        if (value < 0) {
            throw new IllegalArgumentException("Limits cannot be negative");
        }
        return value;
    }

    private static native long getLimit(int limit);

    private static native void setLimit(int limit, long value);
}
