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

extern "C"
{
    AS_API int32_t alphaskia_get_color_type();

    typedef AS_API void *alphaskia_data_t;
    AS_API alphaskia_data_t alphaskia_data_new_copy(const uint8_t *data, uint64_t length);
    AS_API uint8_t *alphaskia_data_get_data(alphaskia_data_t data);
    AS_API uint64_t alphaskia_data_get_length(alphaskia_data_t data);
    AS_API void alphaskia_data_free(alphaskia_data_t data);

    typedef AS_API void *alphaskia_string_t;
    AS_API const char* alphaskia_string_get_utf8(alphaskia_string_t string);
    AS_API uint64_t alphaskia_string_get_length(alphaskia_string_t string);
    AS_API void alphaskia_string_free(alphaskia_string_t string);

    typedef AS_API void *alphaskia_typeface_t;
    AS_API alphaskia_typeface_t alphaskia_typeface_register(alphaskia_data_t data);
    AS_API void alphaskia_typeface_free(alphaskia_typeface_t typeface);
    AS_API alphaskia_typeface_t alphaskia_typeface_make_from_name(const char *family_name, uint8_t bold, uint8_t italic);
    AS_API alphaskia_string_t alphaskia_typeface_get_family_name(alphaskia_typeface_t typeface);
    AS_API uint8_t alphaskia_typeface_is_bold(alphaskia_typeface_t typeface);
    AS_API uint8_t alphaskia_typeface_is_italic(alphaskia_typeface_t typeface);

    typedef AS_API void *alphaskia_image_t;
    AS_API int32_t alphaskia_image_get_width(alphaskia_image_t image);
    AS_API int32_t alphaskia_image_get_height(alphaskia_image_t image);
    AS_API uint8_t alphaskia_image_read_pixels(alphaskia_image_t image, uint8_t *pixels, uint64_t row_bytes);
    AS_API alphaskia_data_t alphaskia_image_encode_png(alphaskia_image_t image);
    AS_API alphaskia_image_t alphaskia_image_decode(const uint8_t *data, uint64_t length);
    AS_API alphaskia_image_t alphaskia_image_from_pixels(int32_t width, int32_t height, const uint8_t *pixels);
    AS_API void alphaskia_image_free(alphaskia_image_t image);

    typedef AS_API void *alphaskia_canvas_t;
    AS_API alphaskia_canvas_t alphaskia_canvas_new();
    AS_API void alphaskia_canvas_free(alphaskia_canvas_t canvas);

    AS_API void alphaskia_canvas_set_color(alphaskia_canvas_t canvas, uint32_t color);
    AS_API uint32_t alphaskia_canvas_get_color(alphaskia_canvas_t canvas);

    AS_API void alphaskia_canvas_set_line_width(alphaskia_canvas_t canvas, float line_width);
    AS_API float alphaskia_canvas_get_line_width(alphaskia_canvas_t canvas);

    AS_API void alphaskia_canvas_begin_render(alphaskia_canvas_t canvas, int32_t width, int32_t height, float render_scale);
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
    AS_API void alphaskia_canvas_fill_text(alphaskia_canvas_t canvas, const char16_t *text, int text_length, alphaskia_typeface_t typeface, float font_size, float x, float y, alphaskia_text_align_t text_align, alphaskia_text_baseline_t baseline);

    AS_API float alphaskia_canvas_measure_text(alphaskia_canvas_t canvas, const char16_t *text, int text_length, alphaskia_typeface_t typeface, float font_size);
    AS_API void alphaskia_canvas_begin_rotate(alphaskia_canvas_t canvas, float center_x, float center_y, float angle);
    AS_API void alphaskia_canvas_end_rotate(alphaskia_canvas_t canvas);
}