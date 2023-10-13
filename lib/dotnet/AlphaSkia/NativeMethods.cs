using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// ReSharper disable InconsistentNaming

namespace AlphaSkia;

internal static partial class NativeMethods
{
    public const string AlphaSkiaNativeLibName = "libalphaskia";

    [LibraryImport(AlphaSkiaNativeLibName)]
    [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
    public static partial int alphaskia_get_color_type();

    [LibraryImport(AlphaSkiaNativeLibName)]
    [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
    public static partial alphaskia_data_t alphaskia_data_new_copy(byte[] data, ulong length);

    [LibraryImport(AlphaSkiaNativeLibName)]
    [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
    public static partial IntPtr alphaskia_data_get_data(alphaskia_data_t data);

    [LibraryImport(AlphaSkiaNativeLibName)]
    [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
    public static partial ulong alphaskia_data_get_length(alphaskia_data_t data);

    [LibraryImport(AlphaSkiaNativeLibName)]
    [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
    public static partial void alphaskia_data_free(alphaskia_data_t data);


    [LibraryImport(AlphaSkiaNativeLibName)]
    [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
    public static partial alphaskia_typeface_t alphaskia_typeface_register(alphaskia_data_t data);

    [LibraryImport(AlphaSkiaNativeLibName)]
    [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
    public static partial void alphaskia_typeface_free(alphaskia_typeface_t type_face);

    [LibraryImport(AlphaSkiaNativeLibName, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
    public static partial alphaskia_typeface_t alphaskia_typeface_make_from_name(string name,
        byte bold, byte italic);


    [LibraryImport(AlphaSkiaNativeLibName)]
    [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
    public static partial void alphaskia_image_free(alphaskia_image_t result);

    [LibraryImport(AlphaSkiaNativeLibName)]
    [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
    public static partial int alphaskia_image_get_width(alphaskia_image_t image);

    [LibraryImport(AlphaSkiaNativeLibName)]
    [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
    public static partial int alphaskia_image_get_height(alphaskia_image_t image);

    [LibraryImport(AlphaSkiaNativeLibName)]
    [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
    public static partial int alphaskia_image_read_pixels(alphaskia_image_t image, IntPtr pixels, ulong rowBytes);

    [LibraryImport(AlphaSkiaNativeLibName)]
    [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
    public static partial alphaskia_data_t alphaskia_image_encode_png(alphaskia_image_t image);

    [LibraryImport(AlphaSkiaNativeLibName)]
    [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
    public static partial alphaskia_canvas_t alphaskia_canvas_new();

    [LibraryImport(AlphaSkiaNativeLibName)]
    [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
    public static partial void alphaskia_canvas_free(alphaskia_canvas_t canvas);

    [LibraryImport(AlphaSkiaNativeLibName)]
    [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
    public static partial void alphaskia_canvas_set_color(alphaskia_canvas_t canvas, uint color);

    [LibraryImport(AlphaSkiaNativeLibName)]
    [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
    public static partial uint alphaskia_canvas_get_color(alphaskia_canvas_t canvas);

    [LibraryImport(AlphaSkiaNativeLibName)]
    [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
    public static partial void alphaskia_canvas_set_line_width(alphaskia_canvas_t canvas, float line_width);

    [LibraryImport(AlphaSkiaNativeLibName)]
    [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
    public static partial float alphaskia_canvas_get_line_width(alphaskia_canvas_t canvas);

    [LibraryImport(AlphaSkiaNativeLibName)]
    [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
    public static partial void alphaskia_canvas_begin_render(alphaskia_canvas_t canvas, int width, int height,
        float render_scale);

    [LibraryImport(AlphaSkiaNativeLibName)]
    [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
    public static partial alphaskia_image_t alphaskia_canvas_end_render(alphaskia_canvas_t canvas);

    [LibraryImport(AlphaSkiaNativeLibName)]
    [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
    public static partial void alphaskia_canvas_fill_rect(alphaskia_canvas_t canvas, float x, float y, float width,
        float height);

    [LibraryImport(AlphaSkiaNativeLibName)]
    [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
    public static partial void alphaskia_canvas_stroke_rect(alphaskia_canvas_t canvas, float x, float y, float width,
        float height);

    [LibraryImport(AlphaSkiaNativeLibName)]
    [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
    public static partial void alphaskia_canvas_begin_path(alphaskia_canvas_t canvas);

    [LibraryImport(AlphaSkiaNativeLibName)]
    [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
    public static partial void alphaskia_canvas_close_path(alphaskia_canvas_t canvas);

    [LibraryImport(AlphaSkiaNativeLibName)]
    [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
    public static partial void alphaskia_canvas_move_to(alphaskia_canvas_t canvas, float x, float y);

    [LibraryImport(AlphaSkiaNativeLibName)]
    [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
    public static partial void alphaskia_canvas_line_to(alphaskia_canvas_t canvas, float x, float y);

    [LibraryImport(AlphaSkiaNativeLibName)]
    [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
    public static partial void alphaskia_canvas_quadratic_curve_to(alphaskia_canvas_t canvas, float cpx, float cpy,
        float x, float y);

    [LibraryImport(AlphaSkiaNativeLibName)]
    [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
    public static partial void alphaskia_canvas_bezier_curve_to(alphaskia_canvas_t canvas, float cp1x, float cp1y,
        float cp2x, float cp2y, float x, float y);

    [LibraryImport(AlphaSkiaNativeLibName)]
    [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
    public static partial void alphaskia_canvas_fill_circle(alphaskia_canvas_t canvas, float x, float y, float radius);

    [LibraryImport(AlphaSkiaNativeLibName)]
    [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
    public static partial void
        alphaskia_canvas_stroke_circle(alphaskia_canvas_t canvas, float x, float y, float radius);

    [LibraryImport(AlphaSkiaNativeLibName)]
    [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
    public static partial void alphaskia_canvas_fill(alphaskia_canvas_t canvas);

    [LibraryImport(AlphaSkiaNativeLibName)]
    [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
    public static partial void alphaskia_canvas_stroke(alphaskia_canvas_t canvas);


    [LibraryImport(AlphaSkiaNativeLibName, StringMarshalling = StringMarshalling.Utf16)]
    [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
    public static partial void alphaskia_canvas_fill_text(alphaskia_canvas_t canvas, string text,
        alphaskia_typeface_t type_face, float font_size, float x, float y, AlphaSkiaTextAlign text_align,
        AlphaSkiaTextBaseline baseline);

    [LibraryImport(AlphaSkiaNativeLibName, StringMarshalling = StringMarshalling.Utf16)]
    [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
    public static partial float alphaskia_canvas_measure_text(alphaskia_canvas_t canvas, string text,
        alphaskia_typeface_t type_face, float font_size);

    [LibraryImport(AlphaSkiaNativeLibName)]
    [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
    public static partial void alphaskia_canvas_begin_rotate(alphaskia_canvas_t canvas, float center_x, float center_y,
        float angle);

    [LibraryImport(AlphaSkiaNativeLibName)]
    [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
    public static partial void alphaskia_canvas_end_rotate(alphaskia_canvas_t canvas);

    [LibraryImport(AlphaSkiaNativeLibName)]
    [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
    public static partial void alphaskia_canvas_draw_image(alphaskia_canvas_t canvas, alphaskia_image_t image, float x,
        float y, float w, float h);
}