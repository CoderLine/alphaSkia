package alphaTab.alphaSkia;

/**
 * Lists the image formats {@link AlphaSkiaImage#decode(byte[])} can decode.
 */
public enum AlphaSkiaImageFormat {
    PNG(1),
    BMP(2),
    GIF(4),
    ICO(8),
    WBMP(16);

    private final int value;

    int getValue() {
        return this.value;
    }

    AlphaSkiaImageFormat(int value) {
        this.value = value;
    }
}
