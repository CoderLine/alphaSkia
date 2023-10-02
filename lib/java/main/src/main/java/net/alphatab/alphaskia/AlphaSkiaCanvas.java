package net.alphatab.alphaskia;

/**
 * A Skia based canvas for rendering images.
 */
public class AlphaSkiaCanvas extends AlphaSkiaNative {
    private static AlphaSkiaColorType colorType;

    /**
     * Gets or sets the {@link AlphaSkiaColorType} used when generating images.
     * AlphaSkia tries to use the native color tpe of the system but this can vary based on OS.
     */
    public static AlphaSkiaColorType getColorType() {
        if (colorType == null) {
            colorType = AlphaSkiaColorType.fromValue(alphaskiaColorType());
        }
        return colorType;
    }

    /**
     * Gets the color to use for drawing operations in the native canvas.
     * See also @{link rgbaToColor}
     */
    public native int getColor();

    /**
     * Sets the color to use for drawing operations in the native canvas.
     * See also @{link rgbaToColor}
     */
    public native void setColor(int color);

    /**
     * Encodes the given color components into the right format.
     *
     * @param r The red component of the color.
     * @param g The green component of the color.
     * @param b The blue component of the color.
     * @param a The alpha channel of the color.
     */
    public static int rgbaToColor(byte r, byte g, byte b, byte a) {
        return switch (getColorType()) {
            case BGRA_8888 -> (((a & 0xFF) << 24) | ((r & 0xFF) << 16) | ((g & 0xFF) << 8) |
                    ((b & 0xFF) << 0));
            case RGBA_8888 -> (((a & 0xFF) << 24) | ((b & 0xFF) << 16) | ((g & 0xFF) << 8) |
                    ((r & 0xFF) << 0));
            default -> throw new IllegalStateException("Unknown color type");
        };
    }

    /**
     * Gets the line width to use when drawing strokes and lines.
     */
    public native float getLineWidth();

    /**
     * Sets the line width to use when drawing strokes and lines.
     */
    public native void setLineWidth(float lineWidth);

    public AlphaSkiaCanvas() {
        super(alphaskiaCanvasAllocate());
    }

    @Override
    public native void close();

    /**
     * Starts a new rendering session in the canvas.
     *
     * @param width  The width of the image to produce.
     * @param height The height of the image to produce.
     */
    public void beginRender(int width, int height)
    {
        beginRender(width, height, 1);
    }

    /**
     * Starts a new rendering session in the canvas.
     *
     * @param width  The width of the image to produce.
     * @param height The height of the image to produce.
     * @param scaleFactor The scale factor for the image (e.g. for high DPI rendering with keeping cooridnatess).
     */
    public native void beginRender(int width, int height, float scaleFactor);

    /**
     * Draws the given image into the canvas.
     *
     * @param image The image to draw.
     * @param x     The X-coordinate at which to draw the image.
     * @param y     The Y-coordinate at which to draw the image.
     * @param w     The target width to which the image should be scaled.
     * @param h     The target height to which the image should be scaled.
     */
    public native void drawImage(AlphaSkiaImage image, float x, float y, float w, float h);

    /**
     * Ends the rendering session and provides the rendered result.
     *
     * @return The rendered result or <code>null</code> if something went wrong.
     */
    public native AlphaSkiaImage endRender();

    /**
     * Fills the given rectangle with the current color.
     *
     * @param x      The X-coordinate of the rectangle
     * @param y      The Y-coordinate of the rectangle
     * @param width  The width of the rectangle
     * @param height The height of the rectangle
     */
    public native void fillRect(float x, float y, float width, float height);

    /**
     * Strokes the given rectangle with the current color.
     *
     * @param x      The X-coordinate of the rectangle
     * @param y      The Y-coordinate of the rectangle
     * @param width  The width of the rectangle
     * @param height The height of the rectangle
     */
    public native void strokeRect(float x, float y, float width, float height);

    /**
     * Begins a new dynamic path for rendering.
     */
    public native void beginPath();

    /**
     * Closes the started path by connecting the last point and the first point.
     */
    public native void closePath();

    /**
     * Moves the current position within the current path to the given position.
     *
     * @param x The X-position where to continue the path.
     * @param y The Y-position where to continue the path.
     */
    public native void moveTo(float x, float y);

    /**
     * Draws a line from the current path position to the given position.
     *
     * @param x The X-position to which to draw a line.
     * @param y The Y-position to which to draw a line.
     */
    public native void lineTo(float x, float y);

    /**
     * Draws a quadratic curve from the current path position to the given position.
     *
     * @param cpx The X-position of the control point.
     * @param cpy The Y-position of the control point.
     * @param x   The X-position of the curve end.
     * @param y   The Y-position of the curve end.
     */
    public native void quadraticCurveTo(float cpx, float cpy, float x, float y);

    /**
     * Draws a bezier curve from the current path position to the given position.
     *
     * @param cp1X The X-position of the first control point.
     * @param cp1Y The Y-position of the first control point.
     * @param cp2X The X-position of the second control point.
     * @param cp2Y The Y-position of the second control point.
     * @param x    The X-position of the curve end.
     * @param y    The Y-position of the curve end.
     */
    public native void bezierCurveTo(float cp1X, float cp1Y, float cp2X, float cp2Y, float x, float y);

    /**
     * Fills a circle with the current color.
     *
     * @param x      The X-position of the circle center.
     * @param y      The Y-position of the circle center.
     * @param radius The circle radius.
     */
    public native void fillCircle(float x, float y, float radius);

    /**
     * Strokes a circle with the current color and line width.
     *
     * @param x      The X-position of the circle center.
     * @param y      The Y-position of the circle center.
     * @param radius The circle radius.
     */
    public native void strokeCircle(float x, float y, float radius);

    /**
     * Fills the current path with the current color.
     */
    public native void fill();

    /**
     * Strokes the current path with the current color and line width.
     */
    public native void stroke();

    /**
     * Fills a text with the current color and provided details.
     *
     * @param text      The text to draw.
     * @param typeface  The typeface to use for drawing the text.
     * @param fontSize  The font size to use when drawing the text.
     * @param x         The X-position where to draw the text to.
     * @param y         The Y-position where to draw the text to.
     * @param textAlign How to align the text at the given position horizontally.
     * @param baseline  How to align the text at the given position vertically.
     */
    public native void fillText(String text, AlphaSkiaTypeface typeface, float fontSize, float x, float y,
                                AlphaSkiaTextAlign textAlign,
                                AlphaSkiaTextBaseline baseline);

    /**
     * Measures the given text.
     *
     * @param text     The text to measure.
     * @param typeface The typeface to use for drawing the text.
     * @param fontSize The font size to use when drawing the text.
     * @return The horizontal width of the text when it would be drawn.
     */
    public native float measureText(String text, AlphaSkiaTypeface typeface, float fontSize);

    /**
     * Rotates the canvas allowing angled drawing. .
     *
     * @param centerX The X-position of the center around which to rotate.
     * @param centerY The Y-position of the center around which to rotate.
     * @param angle   The angle in degrees to rotate.
     */
    public native void beginRotate(float centerX, float centerY, float angle);

    /**
     * Restores the previous rotation state after <see cref="BeginRotate"/> was called.
     */
    public native void endRotate();

    private native static long alphaskiaCanvasAllocate();

    private static native int alphaskiaColorType();
}
