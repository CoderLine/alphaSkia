// ReSharper disable InconsistentNaming

using System.Runtime.InteropServices;

namespace AlphaSkia;

internal static class NativeMethods
{
    public const string AlphaSkiaNativeLibName = "libalphaskia";

    [DllImport(AlphaSkiaNativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern alphaskia_data_t alphaskia_data_new_copy(byte[] data, ulong length);

    [DllImport(AlphaSkiaNativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern int alphaskia_get_color_type();
    [DllImport(AlphaSkiaNativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void alphaskia_switch_to_freetype_fonts();
    [DllImport(AlphaSkiaNativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void alphaskia_switch_to_operating_system_fonts();


    [DllImport(AlphaSkiaNativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr alphaskia_data_get_data(alphaskia_data_t data);

    [DllImport(AlphaSkiaNativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern ulong alphaskia_data_get_length(alphaskia_data_t data);

    [DllImport(AlphaSkiaNativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void alphaskia_data_free(alphaskia_data_t data);

    [DllImport(AlphaSkiaNativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr alphaskia_string_get_utf8(alphaskia_string_t str);
    [DllImport(AlphaSkiaNativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern ulong alphaskia_string_get_length(alphaskia_string_t str);
    [DllImport(AlphaSkiaNativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void alphaskia_string_free(alphaskia_string_t str);


    [DllImport(AlphaSkiaNativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern alphaskia_typeface_t alphaskia_typeface_register(alphaskia_data_t data);
    [DllImport(AlphaSkiaNativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern alphaskia_string_t alphaskia_typeface_get_family_name(alphaskia_typeface_t typeface);
    [DllImport(AlphaSkiaNativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern ushort alphaskia_typeface_get_weight(alphaskia_typeface_t typeface);
    [DllImport(AlphaSkiaNativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern byte alphaskia_typeface_is_italic(alphaskia_typeface_t typeface);

    [DllImport(AlphaSkiaNativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void alphaskia_typeface_free(alphaskia_typeface_t type_face);

    [DllImport(AlphaSkiaNativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern alphaskia_textstyle_t alphaskia_textstyle_new(byte family_name_count, string[] family_names, ushort weight, byte italic);
    [DllImport(AlphaSkiaNativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern byte alphaskia_textstyle_get_family_name_count(alphaskia_textstyle_t textstyle);
    [DllImport(AlphaSkiaNativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern alphaskia_string_t alphaskia_textstyle_get_family_name(alphaskia_textstyle_t textstyle, byte index);
    [DllImport(AlphaSkiaNativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern ushort alphaskia_textstyle_get_weight(alphaskia_textstyle_t textstyle);
    [DllImport(AlphaSkiaNativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern byte alphaskia_textstyle_is_italic(alphaskia_textstyle_t textstyle);
    [DllImport(AlphaSkiaNativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void alphaskia_textstyle_free(alphaskia_textstyle_t textstyle);

    [DllImport(AlphaSkiaNativeLibName)]
    public static extern alphaskia_typeface_t alphaskia_typeface_make_from_name(
#if NETSTANDARD2_0
        [MarshalAs(UnmanagedType.LPStr)]
#else
        [MarshalAs(UnmanagedType.LPUTF8Str)]
        #endif
        string utf8Name,
        ushort weight, byte italic);


    [DllImport(AlphaSkiaNativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void alphaskia_image_free(alphaskia_image_t result);

    [DllImport(AlphaSkiaNativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern alphaskia_image_t alphaskia_image_decode(byte[] data, ulong length);

    [DllImport(AlphaSkiaNativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern alphaskia_image_t alphaskia_image_from_pixels(int width, int height, byte[] pixels);

    [DllImport(AlphaSkiaNativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern int alphaskia_image_get_width(alphaskia_image_t image);

    [DllImport(AlphaSkiaNativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern int alphaskia_image_get_height(alphaskia_image_t image);

    [DllImport(AlphaSkiaNativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern int alphaskia_image_read_pixels(alphaskia_image_t image, IntPtr pixels, ulong rowBytes);


    [DllImport(AlphaSkiaNativeLibName, EntryPoint = nameof(alphaskia_image_read_pixels))]
    public static extern int alphaskia_image_read_pixels_bytes(alphaskia_image_t image, byte[] pixels, ulong rowBytes);

    [DllImport(AlphaSkiaNativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern alphaskia_data_t alphaskia_image_encode_png(alphaskia_image_t image);

    [DllImport(AlphaSkiaNativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern alphaskia_canvas_t alphaskia_canvas_new();

    [DllImport(AlphaSkiaNativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void alphaskia_canvas_free(alphaskia_canvas_t canvas);

    [DllImport(AlphaSkiaNativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void alphaskia_canvas_set_color(alphaskia_canvas_t canvas, uint color);

    [DllImport(AlphaSkiaNativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern uint alphaskia_canvas_get_color(alphaskia_canvas_t canvas);

    [DllImport(AlphaSkiaNativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void alphaskia_canvas_set_line_width(alphaskia_canvas_t canvas, float line_width);

    [DllImport(AlphaSkiaNativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern float alphaskia_canvas_get_line_width(alphaskia_canvas_t canvas);

    [DllImport(AlphaSkiaNativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void alphaskia_canvas_begin_render(alphaskia_canvas_t canvas, int width, int height,
        float render_scale);

    [DllImport(AlphaSkiaNativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern alphaskia_image_t alphaskia_canvas_end_render(alphaskia_canvas_t canvas);

    [DllImport(AlphaSkiaNativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void alphaskia_canvas_fill_rect(alphaskia_canvas_t canvas, float x, float y, float width,
        float height);

    [DllImport(AlphaSkiaNativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void alphaskia_canvas_stroke_rect(alphaskia_canvas_t canvas, float x, float y, float width,
        float height);

    [DllImport(AlphaSkiaNativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void alphaskia_canvas_begin_path(alphaskia_canvas_t canvas);

    [DllImport(AlphaSkiaNativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void alphaskia_canvas_close_path(alphaskia_canvas_t canvas);

    [DllImport(AlphaSkiaNativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void alphaskia_canvas_move_to(alphaskia_canvas_t canvas, float x, float y);

    [DllImport(AlphaSkiaNativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void alphaskia_canvas_line_to(alphaskia_canvas_t canvas, float x, float y);

    [DllImport(AlphaSkiaNativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void alphaskia_canvas_quadratic_curve_to(alphaskia_canvas_t canvas, float cpx, float cpy,
        float x, float y);

    [DllImport(AlphaSkiaNativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void alphaskia_canvas_bezier_curve_to(alphaskia_canvas_t canvas, float cp1x, float cp1y,
        float cp2x, float cp2y, float x, float y);

    [DllImport(AlphaSkiaNativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void alphaskia_canvas_fill_circle(alphaskia_canvas_t canvas, float x, float y, float radius);

    [DllImport(AlphaSkiaNativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void alphaskia_canvas_stroke_circle(alphaskia_canvas_t canvas, float x, float y, float radius);

    [DllImport(AlphaSkiaNativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void alphaskia_canvas_fill(alphaskia_canvas_t canvas);

    [DllImport(AlphaSkiaNativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void alphaskia_canvas_stroke(alphaskia_canvas_t canvas);


    [DllImport(AlphaSkiaNativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void alphaskia_canvas_fill_text(alphaskia_canvas_t canvas,
        [MarshalAs(UnmanagedType.LPWStr)]
        string text,
        int text_length,
        alphaskia_textstyle_t textstyle, float font_size, float x, float y, AlphaSkiaTextAlign text_align,
        AlphaSkiaTextBaseline baseline);

    [DllImport(AlphaSkiaNativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern float alphaskia_canvas_measure_text(alphaskia_canvas_t canvas,
        [MarshalAs(UnmanagedType.LPWStr)]
        string text,
        int text_length,
        alphaskia_textstyle_t textstyle, float font_size);

    [DllImport(AlphaSkiaNativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void alphaskia_canvas_begin_rotate(alphaskia_canvas_t canvas, float center_x, float center_y,
        float angle);

    [DllImport(AlphaSkiaNativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void alphaskia_canvas_end_rotate(alphaskia_canvas_t canvas);

    [DllImport(AlphaSkiaNativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void alphaskia_canvas_draw_image(alphaskia_canvas_t canvas, alphaskia_image_t image, float x,
        float y, float w, float h);
}