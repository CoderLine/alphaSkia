namespace AlphaSkia;

/// <summary>
/// The TextMetrics interface represents the dimensions of a piece of text in the canvas; 
/// A TextMetrics instance can be retrieved using the <see cref="AlphaSkiaCanvas.MeasureText"/> method.
/// </summary>
public sealed class AlphaSkiaTextMetrics : AlphaSkiaNative
{
    private AlphaSkiaTextMetrics(IntPtr handle)
        : base(handle, NativeMethods.alphaskia_text_metrics_free)
    {
    }

    /// <summary>
    /// Returns the width of a segment of inline text in pixels. It takes into account the current font of the context.
    /// </summary>
    public float Width => NativeMethods.alphaskia_text_metrics_get_width(Handle);

    /// <summary>
    /// Distance parallel to the baseline from the alignment point given by the textAlign parameter to the left side of the bounding rectangle of the given text, in pixels; positive numbers indicating a distance going left from the given alignment point.
    /// </summary>
    public float ActualBoundingBoxLeft => NativeMethods.alphaskia_text_metrics_get_actual_bounding_box_left(Handle);

    /// <summary>
    /// Returns the distance from the alignment point given by the textAlign parameter to the right side of the bounding rectangle of the given text, in pixels. The distance is measured parallel to the baseline.
    /// </summary>
    public float ActualBoundingBoxRight => NativeMethods.alphaskia_text_metrics_get_actual_bounding_box_right(Handle);

    /// <summary>
    /// Returns the distance from the horizontal line indicated by the textBaseline parameter to the top of the highest bounding rectangle of all the fonts used to render the text, in pixels.
    /// </summary>
    public float FontBoundingBoxAscent => NativeMethods.alphaskia_text_metrics_get_font_bounding_box_ascent(Handle);

    /// <summary>
    /// Returns the distance from the horizontal line indicated by the textBaseline parameter to the bottom of the bounding rectangle of all the fonts used to render the text, in pixels.
    /// </summary>
    public float FontBoundingBoxDescent => NativeMethods.alphaskia_text_metrics_get_font_bounding_box_descent(Handle);

    /// <summary>
    /// Returns the distance from the horizontal line indicated by the textBaseline parameter to the top of the bounding rectangle used to render the text, in pixels.
    /// </summary>
    public float ActualBoundingBoxAscent => NativeMethods.alphaskia_text_metrics_get_actual_bounding_box_ascent(Handle);

    /// <summary>
    /// Returns the distance from the horizontal line indicated by the textBaseline parameter to the bottom of the bounding rectangle used to render the text, in pixels.
    /// </summary>
    public float ActualBoundingBoxDescent => NativeMethods.alphaskia_text_metrics_get_actual_bounding_box_descent(Handle);

    /// <summary>
    /// Returns the distance from the horizontal line indicated by the textBaseline parameter to the top of the em square in the line box, in pixels.
    /// </summary>
    public float EmHeightAscent => NativeMethods.alphaskia_text_metrics_get_em_height_ascent(Handle);

    /// <summary>
    /// Returns the distance from the horizontal line indicated by the textBaseline parameter to the bottom of the em square in the line box, in pixels.
    /// </summary>
    public float EmHeightDescent => NativeMethods.alphaskia_text_metrics_get_em_height_descent(Handle);

    /// <summary>
    /// Returns the distance from the horizontal line indicated by the textBaseline parameter to the hanging baseline of the line box, in pixels.
    /// </summary>
    public float HangingBaseline => NativeMethods.alphaskia_text_metrics_get_hanging_baseline(Handle);

    /// <summary>
    /// Returns the distance from the horizontal line indicated by the textBaseline parameter to the alphabetic baseline of the line box, in pixels.
    /// </summary>
    public float AlphabeticBaseline => NativeMethods.alphaskia_text_metrics_get_hanging_baseline(Handle);

    /// <summary>
    /// Returns the distance from the horizontal line indicated by the textBaseline parameter to the ideographic baseline of the line box, in CSS pixels.
    /// </summary>
    public float IdeographicBaseline => NativeMethods.alphaskia_text_metrics_get_ideographic_baseline(Handle);
}