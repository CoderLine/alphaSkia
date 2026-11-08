#pragma once

#include <cstdint>

#if !defined(AS_API)
#if defined(ALPHASKIA_DLL)
#if defined(_MSC_VER)
#if ALPHASKIA_IMPLEMENTATION
#define AS_API __declspec(dllexport)
#else
#define AS_API __declspec(dllimport)
#endif
#else
#define AS_API __attribute__((visibility("default")))
#endif
#else
#define AS_API
#endif
#endif

// Contract of the alphaSkia C API:
// - Every function returning a handle transfers ownership, the handle must be freed with the matching *_free function.
//   NULL is returned on invalid input or failure.
// - Every function accepts NULL handles: getters return 0/NULL, other functions do nothing. *_free accepts NULL.
// - Pointers returned by getters (alphaskia_data_get_data, alphaskia_string_get_utf8) stay valid until the owning handle
//   is freed.
// - Handles are independent: a handle can be freed while handles created from it are still in use (e.g. a typeface
//   after registration, an image after end_render or the canvas it came from).
// - A handle must not be used by several threads at the same time. Different handles, the font functions
//   (typeface registration and lookup, switching the font manager) and the limits can be used concurrently.
// - Text is UTF-16 with its length in code units, family names are UTF-8.
extern "C"
{
    // Process-wide limits protecting against untrusted input forcing large allocations.
    // A numeric limit of 0 disables it. Inputs exceeding a limit are rejected like invalid input.
    typedef enum
    {
        // Largest width or height of decoded images, images from pixels and render surfaces (after the render scale)
        // in pixels. Default: 32767
        alphaskia_limit_max_image_dimension = 0,
        // Largest width * height of decoded images, images from pixels and render surfaces. Default: 268435456 (2^28)
        alphaskia_limit_max_image_pixels = 1,
        // Largest font data in bytes accepted by alphaskia_typeface_register. Default: 52428800 (50 MiB)
        alphaskia_limit_max_font_data_length = 2,
        // Largest font size for drawing and measuring text. Default: 4096
        alphaskia_limit_max_font_size = 3,
        // Longest text in UTF-16 code units for drawing and measuring. Default: 0 (unlimited)
        alphaskia_limit_max_text_length = 4,
        // Image formats accepted by alphaskia_image_decode, a combination of alphaskia_image_format_t.
        // Default: alphaskia_image_format_png
        alphaskia_limit_image_formats = 5
    } alphaskia_limit_t;

    typedef enum
    {
        alphaskia_image_format_png = 1,
        alphaskia_image_format_bmp = 2,
        alphaskia_image_format_gif = 4,
        alphaskia_image_format_ico = 8,
        alphaskia_image_format_wbmp = 16
    } alphaskia_image_format_t;

    AS_API uint64_t alphaskia_get_limit(alphaskia_limit_t limit);
    AS_API void alphaskia_set_limit(alphaskia_limit_t limit, uint64_t value);

    AS_API int32_t alphaskia_get_color_type();
    // FreeType mode renders all text with FreeType. Registered fonts take precedence, other families are looked up
    // among the fonts installed on the operating system. Switching the mode purges the font caches.
    AS_API void alphaskia_switch_to_freetype_fonts();
    AS_API void alphaskia_switch_to_operating_system_fonts();
    // Releases cached fonts and glyphs, including the operating system fonts loaded for FreeType mode. Typefaces in
    // use stay valid and fonts are loaded again when needed, registered fonts stay registered. Canvases release their
    // cached fonts on their next text operation.
    AS_API void alphaskia_purge_font_caches();

    typedef AS_API void *alphaskia_data_t;
    // Copies the given bytes.
    AS_API alphaskia_data_t alphaskia_data_new_copy(const uint8_t *data, uint64_t length);
    AS_API const uint8_t *alphaskia_data_get_data(alphaskia_data_t data);
    AS_API uint64_t alphaskia_data_get_length(alphaskia_data_t data);
    AS_API void alphaskia_data_free(alphaskia_data_t data);

    typedef AS_API void *alphaskia_string_t;
    AS_API const char* alphaskia_string_get_utf8(alphaskia_string_t string);
    AS_API uint64_t alphaskia_string_get_length(alphaskia_string_t string);
    AS_API void alphaskia_string_free(alphaskia_string_t string);

    typedef AS_API void *alphaskia_typeface_t;
    // Registers the font for lookups by family name until the process ends; freeing the typeface or the data does not
    // unregister it. Returns NULL if the data is no supported font.
    AS_API alphaskia_typeface_t alphaskia_typeface_register(alphaskia_data_t data);
    AS_API void alphaskia_typeface_free(alphaskia_typeface_t typeface);
    AS_API alphaskia_typeface_t alphaskia_typeface_make_from_name(const char *family_name, uint16_t weight, uint8_t italic);
    AS_API alphaskia_string_t alphaskia_typeface_get_family_name(alphaskia_typeface_t typeface);
    AS_API uint16_t alphaskia_typeface_get_weight(alphaskia_typeface_t typeface);
    AS_API uint8_t alphaskia_typeface_is_italic(alphaskia_typeface_t typeface);

    typedef AS_API void *alphaskia_text_style_t;
    // Copies the family names. Returns NULL for family_names NULL with a family_name_count above 0.
    AS_API alphaskia_text_style_t alphaskia_text_style_new(uint8_t family_name_count, const char** family_names, uint16_t weight, uint8_t italic);
    AS_API uint8_t alphaskia_text_style_get_family_name_count(alphaskia_text_style_t text_style);
    AS_API alphaskia_string_t alphaskia_text_style_get_family_name(alphaskia_text_style_t text_style, uint8_t index);
    AS_API uint16_t alphaskia_text_style_get_weight(alphaskia_text_style_t text_style);
    AS_API uint8_t alphaskia_text_style_is_italic(alphaskia_text_style_t text_style);
    AS_API void alphaskia_text_style_free(alphaskia_text_style_t text_style);

    typedef AS_API void *alphaskia_image_t;
    AS_API int32_t alphaskia_image_get_width(alphaskia_image_t image);
    AS_API int32_t alphaskia_image_get_height(alphaskia_image_t image);
    // Writes the pixels in the format of alphaskia_get_color_type with premultiplied alpha. Returns 0 if row_bytes is
    // below width * 4 or the pixels do not fit into pixels_length bytes.
    AS_API uint8_t alphaskia_image_read_pixels(alphaskia_image_t image, uint8_t *pixels, uint64_t row_bytes, uint64_t pixels_length);
    AS_API alphaskia_data_t alphaskia_image_encode_png(alphaskia_image_t image);
    // Copies the bytes. Returns NULL for unsupported or disabled formats (alphaskia_limit_image_formats) and images
    // beyond the size limits.
    AS_API alphaskia_image_t alphaskia_image_decode(const uint8_t *data, uint64_t length);
    // Copies width * height * 4 bytes in the format of alphaskia_get_color_type with premultiplied alpha. Returns NULL
    // if pixels_length is smaller.
    AS_API alphaskia_image_t alphaskia_image_from_pixels(int32_t width, int32_t height, const uint8_t *pixels, uint64_t pixels_length);
    AS_API void alphaskia_image_free(alphaskia_image_t image);

    typedef AS_API void *alphaskia_text_metrics_t;
    AS_API float alphaskia_text_metrics_get_width(alphaskia_text_metrics_t text_metrics);
    AS_API float alphaskia_text_metrics_get_actual_bounding_box_left(alphaskia_text_metrics_t text_metrics);
    AS_API float alphaskia_text_metrics_get_actual_bounding_box_right(alphaskia_text_metrics_t text_metrics);
    AS_API float alphaskia_text_metrics_get_font_bounding_box_ascent(alphaskia_text_metrics_t text_metrics);
    AS_API float alphaskia_text_metrics_get_font_bounding_box_descent(alphaskia_text_metrics_t text_metrics);
    AS_API float alphaskia_text_metrics_get_actual_bounding_box_ascent(alphaskia_text_metrics_t text_metrics);
    AS_API float alphaskia_text_metrics_get_actual_bounding_box_descent(alphaskia_text_metrics_t text_metrics);
    AS_API float alphaskia_text_metrics_get_em_height_ascent(alphaskia_text_metrics_t text_metrics);
    AS_API float alphaskia_text_metrics_get_em_height_descent(alphaskia_text_metrics_t text_metrics);
    AS_API float alphaskia_text_metrics_get_hanging_baseline(alphaskia_text_metrics_t text_metrics);
    AS_API float alphaskia_text_metrics_get_alphabetic_baseline(alphaskia_text_metrics_t text_metrics);
    AS_API float alphaskia_text_metrics_get_ideographic_baseline(alphaskia_text_metrics_t text_metrics);
    AS_API void alphaskia_text_metrics_free(alphaskia_text_metrics_t text_metrics);

    typedef AS_API void *alphaskia_canvas_t;
    AS_API alphaskia_canvas_t alphaskia_canvas_new();
    AS_API void alphaskia_canvas_free(alphaskia_canvas_t canvas);

    AS_API void alphaskia_canvas_set_color(alphaskia_canvas_t canvas, uint32_t color);
    AS_API uint32_t alphaskia_canvas_get_color(alphaskia_canvas_t canvas);

    AS_API void alphaskia_canvas_set_line_width(alphaskia_canvas_t canvas, float line_width);
    AS_API float alphaskia_canvas_get_line_width(alphaskia_canvas_t canvas);

    // Starts drawing on a new surface of width * height (at least 1) scaled by render_scale. Returns 0, leaving the
    // canvas without surface, if render_scale is not positive and finite or the surface exceeds the limits or cannot be
    // allocated. Without a surface all drawing is a no-op and alphaskia_canvas_end_render returns NULL.
    AS_API uint8_t alphaskia_canvas_begin_render(alphaskia_canvas_t canvas, int32_t width, int32_t height, float render_scale);
    AS_API alphaskia_image_t alphaskia_canvas_end_render(alphaskia_canvas_t canvas);
    AS_API void alphaskia_canvas_fill_rect(alphaskia_canvas_t canvas, float x, float y, float width, float height);
    AS_API void alphaskia_canvas_stroke_rect(alphaskia_canvas_t canvas, float x, float y, float width, float height);
    AS_API void alphaskia_canvas_begin_path(alphaskia_canvas_t canvas);
    AS_API void alphaskia_canvas_close_path(alphaskia_canvas_t canvas);
    AS_API void alphaskia_canvas_move_to(alphaskia_canvas_t canvas, float x, float y);
    AS_API void alphaskia_canvas_line_to(alphaskia_canvas_t canvas, float x, float y);
    AS_API void alphaskia_canvas_quadratic_curve_to(alphaskia_canvas_t canvas, float cpx, float cpy, float x, float y);
    AS_API void alphaskia_canvas_bezier_curve_to(alphaskia_canvas_t canvas, float cp1x, float cp1y, float cp2x, float cp2y, float x, float y);
    AS_API void alphaskia_canvas_fill_circle(alphaskia_canvas_t canvas, float x, float y, float radius);
    AS_API void alphaskia_canvas_stroke_circle(alphaskia_canvas_t canvas, float x, float y, float radius);
    AS_API void alphaskia_canvas_fill(alphaskia_canvas_t canvas);
    AS_API void alphaskia_canvas_stroke(alphaskia_canvas_t canvas);
    AS_API void alphaskia_canvas_draw_image(alphaskia_canvas_t canvas, alphaskia_image_t image, float x, float y, float w, float h);

    typedef enum
    {
        alphaskia_text_align_left = 0,
        alphaskia_text_align_center = 1,
        alphaskia_text_align_right = 2
    } alphaskia_text_align_t;

    typedef enum
    {
        alphaskia_text_baseline_alphabetic = 0,
        alphaskia_text_baseline_top = 1,
        alphaskia_text_baseline_middle = 2,
        alphaskia_text_baseline_bottom = 3
    } alphaskia_text_baseline_t;
    // Text with a negative length, NULL text with a length, a non-finite font size or beyond the text limits is not
    // drawn; alphaskia_canvas_measure_text returns NULL for it.
    AS_API void alphaskia_canvas_fill_text(alphaskia_canvas_t canvas, const char16_t *text, int text_length, alphaskia_text_style_t text_style, float font_size, float x, float y, alphaskia_text_align_t text_align, alphaskia_text_baseline_t baseline);

    AS_API alphaskia_text_metrics_t alphaskia_canvas_measure_text(alphaskia_canvas_t canvas, const char16_t *text, int text_length, alphaskia_text_style_t text_style, float font_size, alphaskia_text_align_t text_align, alphaskia_text_baseline_t baseline);
    AS_API void alphaskia_canvas_begin_rotate(alphaskia_canvas_t canvas, float center_x, float center_y, float angle);
    AS_API void alphaskia_canvas_end_rotate(alphaskia_canvas_t canvas);
}