package alphaTab.alphaSkia

import kotlin.jvm.JvmStatic

/**
 * A Skia based canvas for rendering images.
 */
class AlphaSkiaCanvas : AlphaSkiaNative(NativeMethods.alphaskiaCanvasNew()) {
    companion object {
        @JvmStatic
        private var _colorType: AlphaSkiaColorType? = null

        /**
         * Gets the {@link AlphaSkiaColorType} used when generating images.
         * AlphaSkia tries to use the native color tpe of the system but this can vary based on OS.
         * @return The {@link AlphaSkiaColorType} used when generating images
         */
        @JvmStatic
        fun getColorType(): AlphaSkiaColorType {
            var colorType = this._colorType
            if (colorType == null) {
                colorType = AlphaSkiaColorType.fromValue(NativeMethods.alphaskiaGetColorType())
                    ?: AlphaSkiaColorType.RGBA_8888
                this._colorType = colorType
            }
            return colorType
        }


        /**
         * Encodes the given color components into the right format.
         *
         * @param r The red component of the color.
         * @param g The green component of the color.
         * @param b The blue component of the color.
         * @param a The alpha channel of the color.
         * @return The encoded color.
         */
        @JvmStatic
        fun rgbaToColor(r: Byte, g: Byte, b: Byte, a: Byte): Int {
            val ri = r.toInt()
            val gi = g.toInt()
            val bi = b.toInt()
            val ai = a.toInt()
            return ((ai and 0xFF) shl 24) or ((ri and 0xFF) shl 16) or ((gi and 0xFF) shl 8) or (bi and 0xFF)
        }
    }

    /**
     * Gets or sets the color to use for drawing operations in the native canvas.
     * See also {@link #rgbaToColor}
     * @return The color to use for drawing operations in the native canvas
     */
    var color: Int
        get() = NativeMethods.alphaskiaCanvasGetColor(this.handle)
        set(value) = NativeMethods.alphaskiaCanvasSetColor(this.handle, value)


    /**
     * Gets or sets the line width to use when drawing strokes and lines.
     * @return The line width to use when drawing strokes and lines.
     */
    var lineWidth: Float
        get() = NativeMethods.alphaskiaCanvasGetLineWidth(this.handle)
        set(value) = NativeMethods.alphaskiaCanvasSetLineWidth(this.handle, value)

    override fun close() {
        NativeMethods.alphaskiaCanvasFree(this.handle)
    }

    /**
     * Starts a new rendering session in the canvas.
     *
     * @param width  The width of the image to produce.
     * @param height The height of the image to produce.
     */
    fun beginRender(width: Int, height: Int) {
        beginRender(width, height, 1f)
    }

    /**
     * Starts a new rendering session in the canvas.
     *
     * @param width  The width of the image to produce.
     * @param height The height of the image to produce.
     * @param scaleFactor The scale factor for the image (e.g. for high DPI rendering with keeping coordinates).
     */
    fun beginRender(width: Int, height: Int, scaleFactor: Float) {
        NativeMethods.alphaskiaCanvasBeginRender(this.handle, width, height, scaleFactor)
    }

    /**
     * Draws the given image into the canvas.
     *
     * @param image The image to draw.
     * @param x     The X-coordinate at which to draw the image.
     * @param y     The Y-coordinate at which to draw the image.
     * @param w     The target width to which the image should be scaled.
     * @param h     The target height to which the image should be scaled.
     */
    fun drawImage(image: AlphaSkiaImage, x: Float, y: Float, w: Float, h: Float) {
        NativeMethods.alphaskiaCanvasDrawImage(this.handle, image.handle, x, y, w, h)
    }

    /**
     * Ends the rendering session and provides the rendered result.
     *
     * @return The rendered result or {@code null} if something went wrong.
     */
    fun endRender(): AlphaSkiaImage? {
        val image = NativeMethods.alphaskiaCanvasEndRender(this.handle)
        if (image == 0L) {
            return null
        }
        return AlphaSkiaImage(image)
    }

    /**
     * Fills the given rectangle with the current color.
     *
     * @param x      The X-coordinate of the rectangle
     * @param y      The Y-coordinate of the rectangle
     * @param width  The width of the rectangle
     * @param height The height of the rectangle
     */
    fun fillRect(x: Float, y: Float, width: Float, height: Float) {
        NativeMethods.alphaskiaCanvasFillRect(this.handle, x, y, width, height)
    }

    /**
     * Strokes the given rectangle with the current color.
     *
     * @param x      The X-coordinate of the rectangle
     * @param y      The Y-coordinate of the rectangle
     * @param width  The width of the rectangle
     * @param height The height of the rectangle
     */
    fun strokeRect(x: Float, y: Float, width: Float, height: Float) {
        NativeMethods.alphaskiaCanvasStrokeRect(this.handle, x, y, width, height)
    }

    /**
     * Begins a new dynamic path for rendering.
     */
    fun beginPath() {
        NativeMethods.alphaskiaCanvasBeginPath(this.handle)
    }

    /**
     * Closes the started path by connecting the last point and the first point.
     */
    fun closePath() {
        NativeMethods.alphaskiaCanvasClosePath(this.handle)
    }

    /**
     * Moves the current position within the current path to the given position.
     *
     * @param x The X-position where to continue the path.
     * @param y The Y-position where to continue the path.
     */
    fun moveTo(x: Float, y: Float) {
        NativeMethods.alphaskiaCanvasMoveTo(this.handle, x, y)
    }

    /**
     * Draws a line from the current path position to the given position.
     *
     * @param x The X-position to which to draw a line.
     * @param y The Y-position to which to draw a line.
     */
    fun lineTo(x: Float, y: Float) {
        NativeMethods.alphaskiaCanvasLineTo(this.handle, x, y)
    }

    /**
     * Draws a quadratic curve from the current path position to the given position.
     *
     * @param cpx The X-position of the control point.
     * @param cpy The Y-position of the control point.
     * @param x   The X-position of the curve end.
     * @param y   The Y-position of the curve end.
     */
    fun quadraticCurveTo(cpx: Float, cpy: Float, x: Float, y: Float) {
        NativeMethods.alphaskiaCanvasQuadraticCurveTo(this.handle, cpx, cpy, x, y)
    }

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
    fun bezierCurveTo(
        cp1X: Float,
        cp1Y: Float,
        cp2X: Float,
        cp2Y: Float,
        x: Float,
        y: Float
    ) {
        NativeMethods.alphaskiaCanvasBezierCurveTo(this.handle, cp1X, cp1Y, cp2X, cp2Y, x, y)
    }

    /**
     * Fills a circle with the current color.
     *
     * @param x      The X-position of the circle center.
     * @param y      The Y-position of the circle center.
     * @param radius The circle radius.
     */
    fun fillCircle(x: Float, y: Float, radius: Float) {
        NativeMethods.alphaskiaCanvasFillCircle(this.handle, x, y, radius)
    }

    /**
     * Strokes a circle with the current color and line width.
     *
     * @param x      The X-position of the circle center.
     * @param y      The Y-position of the circle center.
     * @param radius The circle radius.
     */
    fun strokeCircle(x: Float, y: Float, radius: Float) {
        NativeMethods.alphaskiaCanvasStrokeCircle(this.handle, x, y, radius)
    }

    /**
     * Fills the current path with the current color.
     */
    fun fill() {
        NativeMethods.alphaskiaCanvasFill(this.handle)
    }

    /**
     * Strokes the current path with the current color and line width.
     */
    fun stroke() {
        NativeMethods.alphaskiaCanvasStroke(this.handle)
    }

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
    fun fillText(
        text: String, typeface: AlphaSkiaTypeface, fontSize: Float, x: Float, y: Float,
        textAlign: AlphaSkiaTextAlign,
        baseline: AlphaSkiaTextBaseline
    ) {
        NativeMethods.alphaskiaCanvasFillText(
            this.handle, text, typeface.handle, fontSize, x, y, textAlign.value, baseline.value
        )
    }

    /**
     * Measures the given text.
     *
     * @param text     The text to measure.
     * @param typeface The typeface to use for drawing the text.
     * @param fontSize The font size to use when drawing the text.
     * @return The horizontal width of the text when it would be drawn.
     */
    fun measureText(text: String, typeface: AlphaSkiaTypeface, fontSize: Float): Float {
        return NativeMethods.alphaskiaCanvasMeasureText(
            this.handle, text, typeface.handle, fontSize
        )
    }

    /**
     * Rotates the canvas allowing angled drawing. .
     *
     * @param centerX The X-position of the center around which to rotate.
     * @param centerY The Y-position of the center around which to rotate.
     * @param angle   The angle in degrees to rotate.
     */
    fun beginRotate(centerX: Float, centerY: Float, angle: Float) {
        NativeMethods.alphaskiaCanvasBeginRotate(
            this.handle, centerX, centerY, angle
        )
    }

    /**
     * Restores the previous rotation state after {@link #beginRotate} was called.
     */
    fun endRotate() {
        NativeMethods.alphaskiaCanvasEndRotate(
            this.handle
        )
    }
}
