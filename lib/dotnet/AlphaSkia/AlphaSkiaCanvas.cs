namespace AlphaSkia;

/// <summary>
/// A Skia based canvas for rendering images. 
/// </summary>
public sealed class AlphaSkiaCanvas : AlphaSkiaNative
{
    private static AlphaSkiaColorType? _colorType;

    /// <summary>
    /// Gets or sets the <see cref="AlphaSkiaColorType"/> used when generating images.
    /// AlphaSkia tries to use the native color tpe of the system but this can vary based on OS.
    /// </summary>
    public static AlphaSkiaColorType ColorType
    {
        get
        {
            _colorType ??= (AlphaSkiaColorType)NativeMethods.alphaskia_get_color_type();
            return _colorType.Value;
        }
    }

    /// <summary>
    /// Switches the rendering to use the operating system font manager and font rendering. 
    /// This results in a platform specific display of any rendered texts and allows using of any
    /// fonts installed on the system.
    /// </summary>
    public static void SwitchToOperatingSystemFonts()
    {
        NativeMethods.alphaskia_switch_to_operating_system_fonts();
    }

    /// <summary>
    /// Switches the rendering to use the FreeType font manager and font rendering. 
    /// This results in a platform independent display of any rendered texts achieving consistent rendering. 
    /// Operating system fonts cannot be used in this mode.
    /// </summary>
    public static void SwitchToFreeTypeFonts()
    {
        NativeMethods.alphaskia_switch_to_freetype_fonts();
    }

    /// <summary>
    /// Initializes a new empty instance of the <see cref="AlphaSkiaCanvas"/> class.
    /// </summary>
    public AlphaSkiaCanvas()
        : base(NativeMethods.alphaskia_canvas_new(), NativeMethods.alphaskia_canvas_free)
    {
    }

    /// <summary>
    /// Gets or sets the color to use for drawing operations in the native canvas
    /// format.
    /// </summary>
    /// <seealso cref="RgbaToColor"/>
    public uint Color
    {
        get
        {
            CheckDisposed();
            return NativeMethods.alphaskia_canvas_get_color(Handle);
        }
        set
        {
            CheckDisposed();
            NativeMethods.alphaskia_canvas_set_color(Handle, value);
        }
    }

    /// <summary>
    /// Encodes the given color components into the right format for <see cref="Color"/>
    /// </summary>
    /// <param name="r">The red component of the color.</param>
    /// <param name="g">The green component of the color.</param>
    /// <param name="b">The blue component of the color.</param>
    /// <param name="a">The alpha channel of the color.</param>
    /// <returns></returns>
    /// <exception cref="ArgumentOutOfRangeException">If an unknown <see cref="ColorType"/> was detected</exception>
    public static uint RgbaToColor(byte r, byte g, byte b, byte a)
    {
        return (uint)(((a & 0xFF) << 24) | ((r & 0xFF) << 16) | ((g & 0xFF) << 8) |
                                                 ((b & 0xFF) << 0));
    }

    /// <summary>
    /// Gets or sets the line width to use when drawing strokes and lines.
    /// </summary>
    public float LineWidth
    {
        get
        {
            CheckDisposed();
            return NativeMethods.alphaskia_canvas_get_line_width(Handle);
        }
        set
        {
            CheckDisposed();
            NativeMethods.alphaskia_canvas_set_line_width(Handle, value);
        }
    }

    /// <summary>
    /// Starts a new rendering session in the canvas.
    /// </summary>
    /// <param name="width">The width of the image to produce.</param>
    /// <param name="height">The height of the image to produce.</param>
    /// <param name="renderScale">The scale factor for the image (e.g. for high DPI rendering with keeping coordinates).</param>
    public void BeginRender(int width, int height, float renderScale = 1)
    {
        CheckDisposed();
        NativeMethods.alphaskia_canvas_begin_render(Handle, width, height, renderScale);
    }

    /// <summary>
    /// Draws the given image into the canvas.
    /// </summary>
    /// <param name="image">The image to draw.</param>
    /// <param name="x">The X-coordinate at which to draw the image.</param>
    /// <param name="y">The Y-coordinate at which to draw the image.</param>
    /// <param name="w">The target width to which the image should be scaled.</param>
    /// <param name="h">The target height to which the image should be scaled.</param>
    public void DrawImage(AlphaSkiaImage image, float x, float y, float w, float h)
    {
        CheckDisposed();
        NativeMethods.alphaskia_canvas_draw_image(Handle, image.Handle, x, y, w, h);
    }

    /// <summary>
    /// Ends the rendering session and provides the rendered result.
    /// </summary>
    /// <returns>The rendered result or <code>null</code> if something went wrong.</returns>
    public AlphaSkiaImage? EndRender()
    {
        CheckDisposed();
        var image = NativeMethods.alphaskia_canvas_end_render(Handle);
        if (image == IntPtr.Zero)
        {
            return null;
        }

        return new AlphaSkiaImage(image);
    }

    /// <summary>
    /// Fills the given rectangle with the current color.
    /// </summary>
    /// <param name="x">The X-coordinate of the rectangle</param>
    /// <param name="y">The Y-coordinate of the rectangle</param>
    /// <param name="width">The width of the rectangle</param>
    /// <param name="height">The height of the rectangle</param>
    public void FillRect(float x, float y, float width, float height)
    {
        CheckDisposed();
        NativeMethods.alphaskia_canvas_fill_rect(Handle, x, y, width, height);
    }

    /// <summary>
    /// Strokes the given rectangle with the current color.
    /// </summary>
    /// <param name="x">The X-coordinate of the rectangle</param>
    /// <param name="y">The Y-coordinate of the rectangle</param>
    /// <param name="width">The width of the rectangle</param>
    /// <param name="height">The height of the rectangle</param>
    public void StrokeRect(float x, float y, float width, float height)
    {
        CheckDisposed();
        NativeMethods.alphaskia_canvas_stroke_rect(Handle, x, y, width, height);
    }

    /// <summary>
    /// Begins a new dynamic path for rendering.
    /// </summary>
    public void BeginPath()
    {
        CheckDisposed();
        NativeMethods.alphaskia_canvas_begin_path(Handle);
    }

    /// <summary>
    /// Closes the started path by connecting the last point and the first point.
    /// </summary>
    public void ClosePath()
    {
        CheckDisposed();
        NativeMethods.alphaskia_canvas_close_path(Handle);
    }

    /// <summary>
    /// Moves the current position within the current path to the given position.
    /// </summary>
    /// <param name="x">The X-position where to continue the path.</param>
    /// <param name="y">The Y-position where to continue the path.</param>
    public void MoveTo(float x, float y)
    {
        CheckDisposed();
        NativeMethods.alphaskia_canvas_move_to(Handle, x, y);
    }

    /// <summary>
    /// Draws a line from the current path position to the given position.
    /// </summary>
    /// <param name="x">The X-position to which to draw a line.</param>
    /// <param name="y">The Y-position to which to draw a line.</param>
    public void LineTo(float x, float y)
    {
        CheckDisposed();
        NativeMethods.alphaskia_canvas_line_to(Handle, x, y);
    }

    /// <summary>
    /// Draws a quadratic curve from the current path position to the given position.
    /// </summary>
    /// <param name="cpx">The X-position of the control point.</param>
    /// <param name="cpy">The Y-position of the control point.</param>
    /// <param name="x">The X-position of the curve end.</param>
    /// <param name="y">The Y-position of the curve end.</param>
    public void QuadraticCurveTo(float cpx, float cpy, float x, float y)
    {
        CheckDisposed();
        NativeMethods.alphaskia_canvas_quadratic_curve_to(Handle, cpx, cpy, x, y);
    }

    /// <summary>
    /// Draws a Bézier curve from the current path position to the given position.
    /// </summary>
    /// <param name="cp1X">The X-position of the first control point.</param>
    /// <param name="cp1Y">The Y-position of the first control point.</param>
    /// <param name="cp2X">The X-position of the second control point.</param>
    /// <param name="cp2Y">The Y-position of the second control point.</param>
    /// <param name="x">The X-position of the curve end.</param>
    /// <param name="y">The Y-position of the curve end.</param>
    public void BezierCurveTo(float cp1X, float cp1Y, float cp2X, float cp2Y, float x, float y)
    {
        CheckDisposed();
        NativeMethods.alphaskia_canvas_bezier_curve_to(Handle, cp1X, cp1Y, cp2X, cp2Y, x, y);
    }

    /// <summary>
    /// Fills a circle with the current color.
    /// </summary>
    /// <param name="x">The X-position of the circle center.</param>
    /// <param name="y">The Y-position of the circle center.</param>
    /// <param name="radius">The circle radius.</param>
    public void FillCircle(float x, float y, float radius)
    {
        CheckDisposed();
        NativeMethods.alphaskia_canvas_fill_circle(Handle, x, y, radius);
    }

    /// <summary>
    /// Strokes a circle with the current color and line width.
    /// </summary>
    /// <param name="x">The X-position of the circle center.</param>
    /// <param name="y">The Y-position of the circle center.</param>
    /// <param name="radius">The circle radius.</param>
    public void StrokeCircle(float x, float y, float radius)
    {
        CheckDisposed();
        NativeMethods.alphaskia_canvas_fill_circle(Handle, x, y, radius);
    }

    /// <summary>
    /// Fills the current path with the current color.
    /// </summary>
    public void Fill()
    {
        CheckDisposed();
        NativeMethods.alphaskia_canvas_fill(Handle);
    }

    /// <summary>
    /// Strokes the current path with the current color and line width.
    /// </summary>
    public void Stroke()
    {
        CheckDisposed();
        NativeMethods.alphaskia_canvas_stroke(Handle);
    }

    /// <summary>
    /// Fills a text with the current color and provided details.
    /// </summary>
    /// <param name="text">The text to draw.</param>
    /// <param name="textStyle">The text style to use for drawing the text.</param>
    /// <param name="fontSize">The font size to use when drawing the text.</param>
    /// <param name="x">The X-position where to draw the text to.</param>
    /// <param name="y">The Y-position where to draw the text to.</param>
    /// <param name="textAlign">How to align the text at the given position horizontally.</param>
    /// <param name="baseline">How to align the text at the given position vertically.</param>
    public void FillText(string text, AlphaSkiaTextStyle textStyle, float fontSize, float x, float y,
        AlphaSkiaTextAlign textAlign,
        AlphaSkiaTextBaseline baseline)
    {
        CheckDisposed();
        NativeMethods.alphaskia_canvas_fill_text(Handle, text, text.Length, textStyle.Handle, fontSize, x, y, textAlign, baseline);
    }

    /// <summary>
    /// Returns a <see cref="AlphaSkiaTextMetrics"/> object that contains information about the measured text (such as its width, for example).
    /// </summary>
    /// <param name="text">The text to measure.</param>
    /// <param name="textStyle">The text style to use for measuring the text.</param>
    /// <param name="fontSize">The font size to use when measuring the text.</param>
    /// <param name="textAlign">How to align the text at the given position horizontally.</param>
    /// <param name="baseline">How to align the text at the given position vertically.</param>
    /// <returns>The text metrics.</returns>
    public AlphaSkiaTextMetrics MeasureText(string text, AlphaSkiaTextStyle textStyle, float fontSize, AlphaSkiaTextAlign textAlign, AlphaSkiaTextBaseline baseline)
    {
        CheckDisposed();
        return new AlphaSkiaTextMetrics(NativeMethods.alphaskia_canvas_measure_text(Handle, text, text.Length, textStyle.Handle, fontSize, textAlign, baseline));
    }

    /// <summary>
    /// Rotates the canvas allowing angled drawing. . 
    /// </summary>
    /// <param name="centerX">The X-position of the center around which to rotate.</param>
    /// <param name="centerY">The Y-position of the center around which to rotate.</param>
    /// <param name="angle">The angle in degrees to rotate.</param>
    public void BeginRotate(float centerX, float centerY, float angle)
    {
        CheckDisposed();
        NativeMethods.alphaskia_canvas_begin_rotate(Handle, centerX, centerY, angle);
    }

    /// <summary>
    /// Restores the previous rotation state after <see cref="BeginRotate"/> was called.
    /// </summary>
    public void EndRotate()
    {
        CheckDisposed();
        NativeMethods.alphaskia_canvas_end_rotate(Handle);
    }
}