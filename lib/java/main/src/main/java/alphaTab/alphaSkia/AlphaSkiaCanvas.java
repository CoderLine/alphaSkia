package alphaTab.alphaSkia;

/**
 * A Skia based canvas for rendering images.
 */
public class AlphaSkiaCanvas extends AlphaSkiaNative {
    private static AlphaSkiaColorType colorType;

    /**
     * Gets the {@link AlphaSkiaColorType} used when generating images.
     * AlphaSkia tries to use the native color tpe of the system but this can vary based on OS.
     * @return The {@link AlphaSkiaColorType} used when generating images
     */
    public static AlphaSkiaColorType getColorType() {
        if (colorType == null) {
            colorType = AlphaSkiaColorType.fromValue(alphaskiaColorType());
        }
        return colorType;
    }

    /**
     * Gets the color to use for drawing operations in the native canvas.
     * See also {@link #rgbaToColor}
     * @return The color to use for drawing operations in the native canvas
     */
    public native int getColor();

    /**
     * Sets the color to use for drawing operations in the native canvas.
     * See also {@link #rgbaToColor}
     * @param color The color to use for drawing operations in the native canvas.
     */
    public native void setColor(int color);

    /**
     * Encodes the given color components into the right format.
     *
     * @param r The red component of the color.
     * @param g The green component of the color.
     * @param b The blue component of the color.
     * @param a The alpha channel of the color.
     * @return The encoded color.
     */
    public static int rgbaToColor(byte r, byte g, byte b, byte a) {
        return (((a & 0xFF) << 24) | ((r & 0xFF) << 16) | ((g & 0xFF) << 8) |
                    (b & 0xFF));
    }

    /**
     * Gets the line width to use when drawing strokes and lines.
     * @return The line width to use when drawing strokes and lines.
     */
    public native float getLineWidth();

    /**
     * Sets the line width to use when drawing strokes and lines.
     * @param lineWidth The line width to use when drawing strokes and lines
     */
    public native void setLineWidth(float lineWidth);

    /**
     * Initializes a new empty canvas to use for drawing.
     */
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
     * @param scaleFactor The scale factor for the image (e.g. for high DPI rendering with keeping coordinates).
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
     * @return The rendered result or {@code null} if something went wrong.
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
     * Draws a Bézier curve from the current path position to the given position.
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
     * @param textStyle The text style to use for drawing the text.
     * @param fontSize  The font size to use when drawing the text.
     * @param x         The X-position where to draw the text to.
     * @param y         The Y-position where to draw the text to.
     * @param textAlign How to align the text at the given position horizontally.
     * @param baseline  How to align the text at the given position vertically.
     */
    public native void fillText(String text, AlphaSkiaTextStyle textStyle, float fontSize, float x, float y,
                                AlphaSkiaTextAlign textAlign,
                                AlphaSkiaTextBaseline baseline);

    /**
     * Returns a {@see AlphaSkiaTextMetrics} object that contains information about the measured text (such as its width, for example).
     *
     * @param text     The text to measure.
     * @param textStyle The text style to use for measuring the text.
     * @param fontSize The font size to use when measuring the text.
     * @param textAlign How to align the text at the given position horizontally.
     * @param baseline  How to align the text at the given position vertically.
     * @return The text metrics.
     */
    public native AlphaSkiaTextMetrics measureText(String text, AlphaSkiaTextStyle textStyle, float fontSize, 
                                    AlphaSkiaTextAlign textAlign,
                                    AlphaSkiaTextBaseline baseline);

    /**
     * Rotates the canvas allowing angled drawing
     *
     * @param centerX The X-position of the center around which to rotate.
     * @param centerY The Y-position of the center around which to rotate.
     * @param angle   The angle in degrees to rotate.
     */
    public native void beginRotate(float centerX, float centerY, float angle);

    /**
     * Restores the previous rotation state after {@link #beginRotate} was called.
     */
    public native void endRotate();

    private native static long alphaskiaCanvasAllocate();

    private static native int alphaskiaColorType();
    
    /**
     * Switches the rendering to use the operating system font manager and font rendering. 
     * This results in a platform specific display of any rendered texts and allows using of any
     * fonts installed on the system.
     */
    public static native void switchToFreeTypeFonts();
    
    /**
     * Switches the rendering to use the FreeType font manager and font rendering. 
     * This results in a platform independent display of any rendered texts achieving consistent rendering. 
     * Operating system fonts cannot be used in this mode.
     */
    public static native void switchToOperatingSystemFonts();
}
