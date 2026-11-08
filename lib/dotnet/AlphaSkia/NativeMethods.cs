// ReSharper disable InconsistentNaming

using System.Runtime.InteropServices;
using System.Text;

namespace AlphaSkia;

internal static class NativeMethods
{
    public const string AlphaSkiaNativeLibName = "libalphaskia";

    /// <summary>
    /// Allocates a NUL terminated UTF-8 copy of the string, to be freed with <see cref="Marshal.FreeHGlobal"/>.
    /// </summary>
    public static IntPtr AllocUtf8(string value)
    {
        var utf8 = Encoding.UTF8.GetBytes(value + "\0");
        var memory = Marshal.AllocHGlobal(utf8.Length);
        Marshal.Copy(utf8, 0, memory, utf8.Length);
        return memory;
    }

    [DllImport(AlphaSkiaNativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern alphaskia_data_t alphaskia_data_new_copy(byte[] data, ulong length);

    [DllImport(AlphaSkiaNativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern int alphaskia_get_color_type();
    [DllImport(AlphaSkiaNativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void alphaskia_switch_to_freetype_fonts();
    [DllImport(AlphaSkiaNativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void alphaskia_switch_to_operating_system_fonts();


    [DllImport(AlphaSkiaNativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr alphaskia_data_get_data(NativeHandle data);

    [DllImport(AlphaSkiaNativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern ulong alphaskia_data_get_length(NativeHandle data);

    [DllImport(AlphaSkiaNativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void alphaskia_data_free(alphaskia_data_t data);

    [DllImport(AlphaSkiaNativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr alphaskia_string_get_utf8(alphaskia_string_t str);
    [DllImport(AlphaSkiaNativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern ulong alphaskia_string_get_length(alphaskia_string_t str);
    [DllImport(AlphaSkiaNativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void alphaskia_string_free(alphaskia_string_t str);


    [DllImport(AlphaSkiaNativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern alphaskia_typeface_t alphaskia_typeface_register(NativeHandle data);
    [DllImport(AlphaSkiaNativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern alphaskia_string_t alphaskia_typeface_get_family_name(NativeHandle typeface);
    [DllImport(AlphaSkiaNativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern ushort alphaskia_typeface_get_weight(NativeHandle typeface);
    [DllImport(AlphaSkiaNativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern byte alphaskia_typeface_is_italic(NativeHandle typeface);

    [DllImport(AlphaSkiaNativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void alphaskia_typeface_free(alphaskia_typeface_t type_face);

    [DllImport(AlphaSkiaNativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern alphaskia_text_style_t alphaskia_text_style_new(byte family_name_count, IntPtr[] utf8_family_names, ushort weight, byte italic);
    [DllImport(AlphaSkiaNativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern byte alphaskia_text_style_get_family_name_count(NativeHandle text_style);
    [DllImport(AlphaSkiaNativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern alphaskia_string_t alphaskia_text_style_get_family_name(NativeHandle text_style, byte index);
    [DllImport(AlphaSkiaNativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern ushort alphaskia_text_style_get_weight(NativeHandle text_style);
    [DllImport(AlphaSkiaNativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern byte alphaskia_text_style_is_italic(NativeHandle text_style);
    [DllImport(AlphaSkiaNativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void alphaskia_text_style_free(alphaskia_text_style_t text_style);

    [DllImport(AlphaSkiaNativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern alphaskia_typeface_t alphaskia_typeface_make_from_name(byte[] utf8Name, ushort weight, byte italic);


    [DllImport(AlphaSkiaNativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void alphaskia_image_free(alphaskia_image_t result);

    [DllImport(AlphaSkiaNativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern alphaskia_image_t alphaskia_image_decode(byte[] data, ulong length);

    [DllImport(AlphaSkiaNativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern alphaskia_image_t alphaskia_image_from_pixels(int width, int height, byte[] pixels, ulong pixelsLength);

    [DllImport(AlphaSkiaNativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern int alphaskia_image_get_width(NativeHandle image);

    [DllImport(AlphaSkiaNativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern int alphaskia_image_get_height(NativeHandle image);

    [DllImport(AlphaSkiaNativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern byte alphaskia_image_read_pixels(NativeHandle image, IntPtr pixels, ulong rowBytes, ulong pixelsLength);

    [DllImport(AlphaSkiaNativeLibName, EntryPoint = nameof(alphaskia_image_read_pixels), CallingConvention = CallingConvention.Cdecl)]
    public static extern byte alphaskia_image_read_pixels_bytes(NativeHandle image, byte[] pixels, ulong rowBytes, ulong pixelsLength);

    [DllImport(AlphaSkiaNativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern alphaskia_data_t alphaskia_image_encode_png(NativeHandle image);

    [DllImport(AlphaSkiaNativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern alphaskia_canvas_t alphaskia_canvas_new();

    [DllImport(AlphaSkiaNativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void alphaskia_canvas_free(alphaskia_canvas_t canvas);

    [DllImport(AlphaSkiaNativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void alphaskia_canvas_set_color(NativeHandle canvas, uint color);

    [DllImport(AlphaSkiaNativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern uint alphaskia_canvas_get_color(NativeHandle canvas);

    [DllImport(AlphaSkiaNativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void alphaskia_canvas_set_line_width(NativeHandle canvas, float line_width);

    [DllImport(AlphaSkiaNativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern float alphaskia_canvas_get_line_width(NativeHandle canvas);

    [DllImport(AlphaSkiaNativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern byte alphaskia_canvas_begin_render(NativeHandle canvas, int width, int height,
        float render_scale);

    [DllImport(AlphaSkiaNativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern alphaskia_image_t alphaskia_canvas_end_render(NativeHandle canvas);

    [DllImport(AlphaSkiaNativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void alphaskia_canvas_fill_rect(NativeHandle canvas, float x, float y, float width,
        float height);

    [DllImport(AlphaSkiaNativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void alphaskia_canvas_stroke_rect(NativeHandle canvas, float x, float y, float width,
        float height);

    [DllImport(AlphaSkiaNativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void alphaskia_canvas_begin_path(NativeHandle canvas);

    [DllImport(AlphaSkiaNativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void alphaskia_canvas_close_path(NativeHandle canvas);

    [DllImport(AlphaSkiaNativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void alphaskia_canvas_move_to(NativeHandle canvas, float x, float y);

    [DllImport(AlphaSkiaNativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void alphaskia_canvas_line_to(NativeHandle canvas, float x, float y);

    [DllImport(AlphaSkiaNativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void alphaskia_canvas_quadratic_curve_to(NativeHandle canvas, float cpx, float cpy,
        float x, float y);

    [DllImport(AlphaSkiaNativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void alphaskia_canvas_bezier_curve_to(NativeHandle canvas, float cp1x, float cp1y,
        float cp2x, float cp2y, float x, float y);

    [DllImport(AlphaSkiaNativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void alphaskia_canvas_fill_circle(NativeHandle canvas, float x, float y, float radius);

    [DllImport(AlphaSkiaNativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void alphaskia_canvas_stroke_circle(NativeHandle canvas, float x, float y, float radius);

    [DllImport(AlphaSkiaNativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void alphaskia_canvas_fill(NativeHandle canvas);

    [DllImport(AlphaSkiaNativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void alphaskia_canvas_stroke(NativeHandle canvas);


    [DllImport(AlphaSkiaNativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void alphaskia_canvas_fill_text(NativeHandle canvas,
        [MarshalAs(UnmanagedType.LPWStr)]
        string text,
        int text_length,
        NativeHandle text_style, float font_size, float x, float y, AlphaSkiaTextAlign text_align,
        AlphaSkiaTextBaseline baseline);

    [DllImport(AlphaSkiaNativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern alphaskia_text_metrics_t alphaskia_canvas_measure_text(NativeHandle canvas,
        [MarshalAs(UnmanagedType.LPWStr)]
        string text,
        int text_length,
        NativeHandle text_style, float font_size, AlphaSkiaTextAlign text_align,
        AlphaSkiaTextBaseline baseline);

    [DllImport(AlphaSkiaNativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void alphaskia_canvas_begin_rotate(NativeHandle canvas, float center_x, float center_y,
        float angle);

    [DllImport(AlphaSkiaNativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void alphaskia_canvas_end_rotate(NativeHandle canvas);

    [DllImport(AlphaSkiaNativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void alphaskia_canvas_draw_image(NativeHandle canvas, NativeHandle image, float x,
        float y, float w, float h);

    [DllImport(AlphaSkiaNativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern float alphaskia_text_metrics_get_width(NativeHandle text_metrics);

    [DllImport(AlphaSkiaNativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern float alphaskia_text_metrics_get_actual_bounding_box_left(NativeHandle text_metrics);

    [DllImport(AlphaSkiaNativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern float alphaskia_text_metrics_get_actual_bounding_box_right(NativeHandle text_metrics);

    [DllImport(AlphaSkiaNativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern float alphaskia_text_metrics_get_font_bounding_box_ascent(NativeHandle text_metrics);

    [DllImport(AlphaSkiaNativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern float alphaskia_text_metrics_get_font_bounding_box_descent(NativeHandle text_metrics);

    [DllImport(AlphaSkiaNativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern float alphaskia_text_metrics_get_actual_bounding_box_ascent(NativeHandle text_metrics);

    [DllImport(AlphaSkiaNativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern float alphaskia_text_metrics_get_actual_bounding_box_descent(NativeHandle text_metrics);

    [DllImport(AlphaSkiaNativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern float alphaskia_text_metrics_get_em_height_ascent(NativeHandle text_metrics);

    [DllImport(AlphaSkiaNativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern float alphaskia_text_metrics_get_em_height_descent(NativeHandle text_metrics);

    [DllImport(AlphaSkiaNativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern float alphaskia_text_metrics_get_hanging_baseline(NativeHandle text_metrics);

    [DllImport(AlphaSkiaNativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern float alphaskia_text_metrics_get_alphabetic_baseline(NativeHandle text_metrics);

    [DllImport(AlphaSkiaNativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern float alphaskia_text_metrics_get_ideographic_baseline(NativeHandle text_metrics);

    [DllImport(AlphaSkiaNativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void alphaskia_text_metrics_free(alphaskia_text_metrics_t text_metrics);
}