package alphaTab.alphaSkia;

/**
 * The TextMetrics interface represents the dimensions of a piece of text in the canvas;
 * A TextMetrics instance can be retrieved using the {@see AlphaSkiaCanvas.measureText} method.
 */
public class AlphaSkiaTextMetrics extends AlphaSkiaNative {

    /**
     * Returns the width of a segment of inline text in pixels. It takes into account the current font of the context.
     */
    public native float getWidth();

    /**
     * Distance parallel to the baseline from the alignment point given by the textAlign parameter to the left side of the bounding rectangle of the given text, in pixels; positive numbers indicating a distance going left from the given alignment point.
     */
    public native float getActualBoundingBoxLeft();

    /**
     * Returns the distance from the alignment point given by the textAlign parameter to the right side of the bounding rectangle of the given text, in pixels. The distance is measured parallel to the baseline.
     */
    public native float getActualBoundingBoxRight();

    /**
     * Returns the distance from the horizontal line indicated by the textBaseline parameter to the top of the highest bounding rectangle of all the fonts used to render the text, in pixels.
     */
    public native float getFontBoundingBoxAscent();

    /**
     * Returns the distance from the horizontal line indicated by the textBaseline parameter to the bottom of the bounding rectangle of all the fonts used to render the text, in pixels.
     */
    public native float getFontBoundingBoxDescent();

    /**
     * Returns the distance from the horizontal line indicated by the textBaseline parameter to the top of the bounding rectangle used to render the text, in pixels.
     */
    public native float getActualBoundingBoxAscent();

    /**
     * Returns the distance from the horizontal line indicated by the textBaseline parameter to the bottom of the bounding rectangle used to render the text, in pixels.
     */
    public native float getActualBoundingBoxDescent();

    /**
     * Returns the distance from the horizontal line indicated by the textBaseline parameter to the top of the em square in the line box, in pixels.
     */
    public native float getEmHeightAscent();

    /**
     * Returns the distance from the horizontal line indicated by the textBaseline parameter to the bottom of the em square in the line box, in pixels.
     */
    public native float getEmHeightDescent();

    /**
     * Returns the distance from the horizontal line indicated by the textBaseline parameter to the hanging baseline of the line box, in pixels.
     */
    public native float getHangingBaseline();

    /**
     * Returns the distance from the horizontal line indicated by the textBaseline parameter to the alphabetic baseline of the line box, in pixels.
     */
    public native float getAlphabeticBaseline();

    /**
     * Returns the distance from the horizontal line indicated by the textBaseline parameter to the ideographic baseline of the line box, in CSS pixels.
     */
    public native float getIdeographicBaseline();

    AlphaSkiaTextMetrics(long handle) {
        super(handle);
    }

    @Override
    public native void close();
}