#include "../include/AlphaSkiaCanvas.h"
#include "../include/SkFontMgr_alphaskia.h"

#include "../../externals/skia/include/core/SkFontMgr.h"

// C - API
extern "C"
{
    AS_API alphaskia_canvas_t alphaskia_canvas_new()
    {
        AlphaSkiaCanvas *canvas = new AlphaSkiaCanvas();
        return reinterpret_cast<alphaskia_canvas_t>(canvas);
    }

    AS_API void alphaskia_canvas_free(alphaskia_canvas_t canvas)
    {
        AlphaSkiaCanvas *internal = reinterpret_cast<AlphaSkiaCanvas *>(canvas);
        delete internal;
    }

    AS_API void alphaskia_switch_to_freetype_fonts()
    {
        SkFontMgr_AlphaSkia::instance()->switch_to_freetype_fonts();
    }

    AS_API void alphaskia_switch_to_operating_system_fonts()
    {
        SkFontMgr_AlphaSkia::instance()->switch_to_operating_system_fonts();
    }

    AS_API int32_t alphaskia_get_color_type()
    {
        return kN32_SkColorType;
    }

    AS_API void alphaskia_canvas_set_color(alphaskia_canvas_t canvas, uint32_t color)
    {
        reinterpret_cast<AlphaSkiaCanvas *>(canvas)->set_color(color);
    }

    AS_API uint32_t alphaskia_canvas_get_color(alphaskia_canvas_t canvas)
    {
        return reinterpret_cast<AlphaSkiaCanvas *>(canvas)->get_color();
    }

    AS_API void alphaskia_canvas_set_line_width(alphaskia_canvas_t canvas, float line_width)
    {
        reinterpret_cast<AlphaSkiaCanvas *>(canvas)->set_line_width(line_width);
    }

    AS_API float alphaskia_canvas_get_line_width(alphaskia_canvas_t canvas)
    {
        return reinterpret_cast<AlphaSkiaCanvas *>(canvas)->get_line_width();
    }

    AS_API void alphaskia_canvas_begin_render(alphaskia_canvas_t canvas, int32_t width, int32_t height, float render_scale)
    {
        reinterpret_cast<AlphaSkiaCanvas *>(canvas)->begin_render(width, height, render_scale);
    }

    AS_API alphaskia_image_t alphaskia_canvas_end_render(alphaskia_canvas_t canvas)
    {
        sk_sp<SkImage> image = reinterpret_cast<AlphaSkiaCanvas *>(canvas)->end_render();
        return reinterpret_cast<alphaskia_image_t>(new sk_sp<SkImage>(image));
    }

    AS_API void alphaskia_canvas_fill_rect(alphaskia_canvas_t canvas, float x, float y, float width, float height)
    {
        reinterpret_cast<AlphaSkiaCanvas *>(canvas)->fill_rect(x, y, width, height);
    }

    AS_API void alphaskia_canvas_stroke_rect(alphaskia_canvas_t canvas, float x, float y, float width, float height)
    {
        reinterpret_cast<AlphaSkiaCanvas *>(canvas)->stroke_rect(x, y, width, height);
    }

    AS_API void alphaskia_canvas_begin_path(alphaskia_canvas_t canvas)
    {
        reinterpret_cast<AlphaSkiaCanvas *>(canvas)->begin_path();
    }

    AS_API void alphaskia_canvas_close_path(alphaskia_canvas_t canvas)
    {
        reinterpret_cast<AlphaSkiaCanvas *>(canvas)->close_path();
    }

    AS_API void alphaskia_canvas_draw_image(alphaskia_canvas_t canvas, alphaskia_image_t image, float x, float y, float w, float h)
    {
        sk_sp<SkImage> *internalImage = reinterpret_cast<sk_sp<SkImage> *>(image);
        reinterpret_cast<AlphaSkiaCanvas *>(canvas)->draw_image(*internalImage, x, y, w, h);
    }

    AS_API void alphaskia_canvas_move_to(alphaskia_canvas_t canvas, float x, float y)
    {
        reinterpret_cast<AlphaSkiaCanvas *>(canvas)->move_to(x, y);
    }

    AS_API void alphaskia_canvas_line_to(alphaskia_canvas_t canvas, float x, float y)
    {
        reinterpret_cast<AlphaSkiaCanvas *>(canvas)->line_to(x, y);
    }

    AS_API void alphaskia_canvas_quadratic_curve_to(alphaskia_canvas_t canvas, float cpx, float cpy, float x, float y)
    {
        reinterpret_cast<AlphaSkiaCanvas *>(canvas)->quadratic_curve_to(cpx, cpy, x, y);
    }

    AS_API void alphaskia_canvas_bezier_curve_to(alphaskia_canvas_t canvas, float cp1x, float cp1y, float cp2x, float cp2y, float x, float y)
    {
        reinterpret_cast<AlphaSkiaCanvas *>(canvas)->bezier_curve_to(cp1x, cp1y, cp2x, cp2y, x, y);
    }

    AS_API void alphaskia_canvas_fill_circle(alphaskia_canvas_t canvas, float x, float y, float radius)
    {
        reinterpret_cast<AlphaSkiaCanvas *>(canvas)->fill_circle(x, y, radius);
    }

    AS_API void alphaskia_canvas_stroke_circle(alphaskia_canvas_t canvas, float x, float y, float radius)
    {
        reinterpret_cast<AlphaSkiaCanvas *>(canvas)->stroke_circle(x, y, radius);
    }

    AS_API void alphaskia_canvas_fill(alphaskia_canvas_t canvas)
    {
        reinterpret_cast<AlphaSkiaCanvas *>(canvas)->fill();
    }

    AS_API void alphaskia_canvas_stroke(alphaskia_canvas_t canvas)
    {
        reinterpret_cast<AlphaSkiaCanvas *>(canvas)->stroke();
    }

    AS_API void alphaskia_canvas_fill_text(alphaskia_canvas_t canvas, const char16_t *text, int text_length, alphaskia_textstyle_t textstyle, float font_size, float x, float y, alphaskia_text_align_t text_align, alphaskia_text_baseline_t baseline)
    {
        AlphaSkiaTextStyle *skTextstyle = reinterpret_cast<AlphaSkiaTextStyle *>(textstyle);
        reinterpret_cast<AlphaSkiaCanvas *>(canvas)->fill_text(text, text_length, *skTextstyle, font_size, x, y, text_align, baseline);
    }

    AS_API float alphaskia_canvas_measure_text(alphaskia_canvas_t canvas, const char16_t *text, int text_length, alphaskia_textstyle_t textstyle, float font_size)
    {
        AlphaSkiaTextStyle *skTextstyle = reinterpret_cast<AlphaSkiaTextStyle *>(textstyle);
        return reinterpret_cast<AlphaSkiaCanvas *>(canvas)->measure_text(text, text_length, *skTextstyle, font_size);
    }

    AS_API void alphaskia_canvas_begin_rotate(alphaskia_canvas_t canvas, float center_x, float center_y, float angle)
    {
        reinterpret_cast<AlphaSkiaCanvas *>(canvas)->begin_rotate(center_x, center_y, angle);
    }

    AS_API void alphaskia_canvas_end_rotate(alphaskia_canvas_t canvas)
    {
        reinterpret_cast<AlphaSkiaCanvas *>(canvas)->end_rotate();
    }
}