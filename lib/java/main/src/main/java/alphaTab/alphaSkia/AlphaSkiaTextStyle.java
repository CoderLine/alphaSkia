package alphaTab.alphaSkia;

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
     */
    public AlphaSkiaTextStyle(String[] fontFamilies, int weight, boolean isItalic) {
        super(alphaskiaTextStyleNew(fontFamilies, weight, isItalic));
        this.fontFamilies = fontFamilies;
        this.weight = weight;
        this.isItalic = isItalic;
    }

    private native static long alphaskiaTextStyleNew(String[] fontFamilies, int weight, boolean isItalic);

    @Override
    public native void close();
}
