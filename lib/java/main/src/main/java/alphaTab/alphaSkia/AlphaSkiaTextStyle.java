package alphaTab.alphaSkia;

import java.nio.charset.StandardCharsets;
import java.util.Objects;

/**
 * Represents a text style which can be used to draw or measure texts
 * with support for mixed fonts for fallback character rendering.
 */
public class AlphaSkiaTextStyle extends AlphaSkiaNative {
    private final String[] fontFamilies;
    private final int weight;
    private final boolean isItalic;
    /**
     * Gets the list of font family names which are consulted for finding 
     * typefaces with glyphs for drawing or measuring texts.
     * @return The list of font family names
     */
    public String[] getFontFamilies() { return this.fontFamilies; }

    /**
     * Gets the font weight used for finding typefaces.
     * @return The font weight
     */
    public int getWeight() { return this.weight; }

    /**
     * Gets whether the used typefaces should be italic. 
     * @return Whether the used typefaces should be italic
     */
    public boolean isItalic() { return this.isItalic; }

    /**
     * Initializes a new instance of the {@see AlphaSkiaTextStyle} class.
     * @param fontFamilies The list of font family names.
     * @param weight The font weight typefaces should have..
     * @param isItalic Whether typefaces should be italic.
     * @throws IllegalArgumentException if more than 255 font families are given or the weight is outside of 0..1000.
     */
    public AlphaSkiaTextStyle(String[] fontFamilies, int weight, boolean isItalic) {
        super(alphaskiaTextStyleNew(encodeFamilyNames(fontFamilies), checkWeight(weight), isItalic));
        this.fontFamilies = fontFamilies.clone();
        this.weight = weight;
        this.isItalic = isItalic;
    }

    static int checkWeight(int weight) {
        if (weight < 0 || weight > 1000) {
            throw new IllegalArgumentException("weight must be within 0..1000");
        }
        return weight;
    }

    private static byte[][] encodeFamilyNames(String[] fontFamilies) {
        Objects.requireNonNull(fontFamilies, "fontFamilies");
        if (fontFamilies.length > 255) {
            throw new IllegalArgumentException("At most 255 font families are supported");
        }
        var encoded = new byte[fontFamilies.length][];
        for (int i = 0; i < fontFamilies.length; i++) {
            encoded[i] = Objects.requireNonNull(fontFamilies[i], "fontFamilies[" + i + "]").getBytes(StandardCharsets.UTF_8);
        }
        return encoded;
    }

    private native static long alphaskiaTextStyleNew(byte[][] utf8FontFamilies, int weight, boolean isItalic);

    @Override
    public void close() {
        release(takeHandle());
    }

    private static native void release(long handle);
}
