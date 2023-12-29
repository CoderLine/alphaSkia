package alphaTab.alphaSkia

/**
 * A Skia based canvas for rendering images.
 */
class AlphaSkiaCanvas : AlphaSkiaNative(alphaskiaCanvasAllocate()) {
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
                colorType = AlphaSkiaColorType.fromValue(alphaskiaColorType())
                    ?: AlphaSkiaColorType.RGBA_8888
                this._colorType = colorType
            }
            return colorType
        }

        @JvmStatic
        private external fun alphaskiaColorType(): Int


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

        @JvmStatic
        private external fun alphaskiaCanvasAllocate(): Long
    }

    /**
     * Gets the color to use for drawing operations in the native canvas.
     * See also {@link #rgbaToColor}
     * @return The color to use for drawing operations in the native canvas
     */
    external fun getColor(): Int

    /**
     * Sets the color to use for drawing operations in the native canvas.
     * See also {@link #rgbaToColor}
     * @param color The color to use for drawing operations in the native canvas.
     */
    external fun setColor(color: Int)


    /**
     * Gets the line width to use when drawing strokes and lines.
     * @return The line width to use when drawing strokes and lines.
     */
    external fun getLineWidth(): Float

    /**
     * Sets the line width to use when drawing strokes and lines.
     * @param lineWidth The line width to use when drawing strokes and lines
     */
    external fun setLineWidth(lineWidth: Float)

    external override fun close()

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
    external fun beginRender(width: Int, height: Int, scaleFactor: Float)

    /**
     * Draws the given image into the canvas.
     *
     * @param image The image to draw.
     * @param x     The X-coordinate at which to draw the image.
     * @param y     The Y-coordinate at which to draw the image.
     * @param w     The target width to which the image should be scaled.
     * @param h     The target height to which the image should be scaled.
     */
    external fun drawImage(image: AlphaSkiaImage, x: Float, y: Float, w: Float, h: Float)

    /**
     * Ends the rendering session and provides the rendered result.
     *
     * @return The rendered result or {@code null} if something went wrong.
     */
    external fun endRender(): AlphaSkiaImage?

    /**
     * Fills the given rectangle with the current color.
     *
     * @param x      The X-coordinate of the rectangle
     * @param y      The Y-coordinate of the rectangle
     * @param width  The width of the rectangle
     * @param height The height of the rectangle
     */
    external fun fillRect(x: Float, y: Float, width: Float, height: Float)

    /**
     * Strokes the given rectangle with the current color.
     *
     * @param x      The X-coordinate of the rectangle
     * @param y      The Y-coordinate of the rectangle
     * @param width  The width of the rectangle
     * @param height The height of the rectangle
     */
    external fun strokeRect(x: Float, y: Float, width: Float, height: Float)

    /**
     * Begins a new dynamic path for rendering.
     */
    external fun beginPath()

    /**
     * Closes the started path by connecting the last point and the first point.
     */
    external fun closePath()

    /**
     * Moves the current position within the current path to the given position.
     *
     * @param x The X-position where to continue the path.
     * @param y The Y-position where to continue the path.
     */
    external fun moveTo(x: Float, y: Float)

    /**
     * Draws a line from the current path position to the given position.
     *
     * @param x The X-position to which to draw a line.
     * @param y The Y-position to which to draw a line.
     */
    external fun lineTo(x: Float, y: Float)

    /**
     * Draws a quadratic curve from the current path position to the given position.
     *
     * @param cpx The X-position of the control point.
     * @param cpy The Y-position of the control point.
     * @param x   The X-position of the curve end.
     * @param y   The Y-position of the curve end.
     */
    external fun quadraticCurveTo(cpx: Float, cpy: Float, x: Float, y: Float)

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
    external fun bezierCurveTo(
        cp1X: Float,
        cp1Y: Float,
        cp2X: Float,
        cp2Y: Float,
        x: Float,
        y: Float
    )

    /**
     * Fills a circle with the current color.
     *
     * @param x      The X-position of the circle center.
     * @param y      The Y-position of the circle center.
     * @param radius The circle radius.
     */
    external fun fillCircle(x: Float, y: Float, radius: Float)

    /**
     * Strokes a circle with the current color and line width.
     *
     * @param x      The X-position of the circle center.
     * @param y      The Y-position of the circle center.
     * @param radius The circle radius.
     */
    external fun strokeCircle(x: Float, y: Float, radius: Float)

    /**
     * Fills the current path with the current color.
     */
    external fun fill()

    /**
     * Strokes the current path with the current color and line width.
     */
    external fun stroke()

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
    external fun fillText(
        text: String, typeface: AlphaSkiaTypeface, fontSize: Float, x: Float, y: Float,
        textAlign: AlphaSkiaTextAlign,
        baseline: AlphaSkiaTextBaseline
    )

    /**
     * Measures the given text.
     *
     * @param text     The text to measure.
     * @param typeface The typeface to use for drawing the text.
     * @param fontSize The font size to use when drawing the text.
     * @return The horizontal width of the text when it would be drawn.
     */
    external fun measureText(text: String, typeface: AlphaSkiaTypeface, fontSize: Float): Float

    /**
     * Rotates the canvas allowing angled drawing. .
     *
     * @param centerX The X-position of the center around which to rotate.
     * @param centerY The Y-position of the center around which to rotate.
     * @param angle   The angle in degrees to rotate.
     */
    external fun beginRotate(centerX: Float, centerY: Float, angle: Float)

    /**
     * Restores the previous rotation state after {@link #beginRotate} was called.
     */
    external fun endRotate()
}
