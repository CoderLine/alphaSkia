package net.alphatab.alphaskia;

/**
 * Represents a typeface to draw text.
 */
public class AlphaSkiaTypeface extends AlphaSkiaNative {
    private final AlphaSkiaData data;

    private String familyName;

    /**
     * Gets the name of the font family of this typeface.
     *
     * @return The name of the font family of this typeface
     */
    public String getFamilyName() {
        if(this.familyName == null){
            this.familyName = AlphaSkiaTypeface.loadFamilyName(this.handle);
        }
        return this.familyName;
    }

    /**
     * Gets a value indicating whether the typeface is bold.
     * @return true if the font is bold, otherwise false.
     */
    public native boolean isBold();

    /**
     * Gets a value indicating whether the typeface is italic.
     * @return true if the font is italic, otherwise false.
     */
    public native boolean isItalic();

    private AlphaSkiaTypeface(long handle, AlphaSkiaData data) {
        super(handle);
        this.data = data;
    }

    @Override
    public void close() {
        AlphaSkiaData data = this.data;
        if (data != null) {
            data.close();
        }
        release(handle);
        handle = 0;
    }

    private static native String loadFamilyName(long handle);
    private native void release(long handle);

    /**
     * Register a new custom font from the given binary data containing the data of a font compatible with Skia (e.g. TTF).
     *
     * @param data The raw binary data of the font.
     * @return The loaded typeface to use for text rendering or {@code null} if the loading failed.
     */
    public static AlphaSkiaTypeface register(byte[] data) {
        var nativeData = new AlphaSkiaData(data);
        var typeface = register(nativeData.handle);
        if (typeface == 0) {
            nativeData.close();
            return null;
        }
        return new AlphaSkiaTypeface(typeface, nativeData);
    }

    private static native long register(long handle);

    /**
     * Creates a typeface using the provided information.
     *
     * @param name   The name of the typeface.
     * @param bold   Whether the bold version of the typeface should be loaded.
     * @param italic Whether the italic version of the typeface should be loaded.
     * @return The typeface if it can be found in the already loaded fonts or the system fonts, otherwise {@code null}.
     */
    public static AlphaSkiaTypeface create(String name, boolean bold, boolean italic) {
        var typeface = makeFromName(name, bold, italic);
        if (typeface == 0) {
            return null;
        }

        return new AlphaSkiaTypeface(typeface, null);
    }

    private static native long makeFromName(String name, boolean bold, boolean italic);
}
