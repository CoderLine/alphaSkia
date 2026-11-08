#include "../include/AlphaSkiaCanvas.h"
#include "../include/AlphaSkiaLimits.h"
#include "../include/SkFontMgr_alphaskia.h"

#include "../../externals/skia/include/core/SkFontMgr.h"

#include <cmath>

namespace
{
    AlphaSkiaCanvas *get_canvas(alphaskia_canvas_t canvas)
    {
        return reinterpret_cast<AlphaSkiaCanvas *>(canvas);
    }

    bool is_valid_text(const char16_t *text, int text_length, alphaskia_text_style_t text_style, float font_size)
    {
        return text_style && text_length >= 0 && (text || text_length == 0) && std::isfinite(font_size) &&
               alphaskia_is_within_text_limits(text_length, font_size);
    }
}

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
        delete get_canvas(canvas);
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
        if (!canvas)
        {
            return;
        }
        get_canvas(canvas)->set_color(color);
    }

    AS_API uint32_t alphaskia_canvas_get_color(alphaskia_canvas_t canvas)
    {
        if (!canvas)
        {
            return 0;
        }
        return get_canvas(canvas)->get_color();
    }

    AS_API void alphaskia_canvas_set_line_width(alphaskia_canvas_t canvas, float line_width)
    {
        if (!canvas)
        {
            return;
        }
        get_canvas(canvas)->set_line_width(line_width);
    }

    AS_API float alphaskia_canvas_get_line_width(alphaskia_canvas_t canvas)
    {
        if (!canvas)
        {
            return 0;
        }
        return get_canvas(canvas)->get_line_width();
    }

    AS_API uint8_t alphaskia_canvas_begin_render(alphaskia_canvas_t canvas, int32_t width, int32_t height, float render_scale)
    {
        if (!canvas)
        {
            return 0;
        }
        return get_canvas(canvas)->begin_render(width, height, render_scale) ? 1 : 0;
    }

    AS_API alphaskia_image_t alphaskia_canvas_end_render(alphaskia_canvas_t canvas)
    {
        if (!canvas)
        {
            return nullptr;
        }

        sk_sp<SkImage> image = get_canvas(canvas)->end_render();
        if (!image)
        {
            return nullptr;
        }
        return reinterpret_cast<alphaskia_image_t>(new sk_sp<SkImage>(image));
    }

    AS_API void alphaskia_canvas_fill_rect(alphaskia_canvas_t canvas, float x, float y, float width, float height)
    {
        if (!canvas)
        {
            return;
        }
        get_canvas(canvas)->fill_rect(x, y, width, height);
    }

    AS_API void alphaskia_canvas_stroke_rect(alphaskia_canvas_t canvas, float x, float y, float width, float height)
    {
        if (!canvas)
        {
            return;
        }
        get_canvas(canvas)->stroke_rect(x, y, width, height);
    }

    AS_API void alphaskia_canvas_begin_path(alphaskia_canvas_t canvas)
    {
        if (!canvas)
        {
            return;
        }
        get_canvas(canvas)->begin_path();
    }

    AS_API void alphaskia_canvas_close_path(alphaskia_canvas_t canvas)
    {
        if (!canvas)
        {
            return;
        }
        get_canvas(canvas)->close_path();
    }

    AS_API void alphaskia_canvas_draw_image(alphaskia_canvas_t canvas, alphaskia_image_t image, float x, float y, float w, float h)
    {
        if (!canvas || !image)
        {
            return;
        }

        sk_sp<SkImage> *internalImage = reinterpret_cast<sk_sp<SkImage> *>(image);
        get_canvas(canvas)->draw_image(*internalImage, x, y, w, h);
    }

    AS_API void alphaskia_canvas_move_to(alphaskia_canvas_t canvas, float x, float y)
    {
        if (!canvas)
        {
            return;
        }
        get_canvas(canvas)->move_to(x, y);
    }

    AS_API void alphaskia_canvas_line_to(alphaskia_canvas_t canvas, float x, float y)
    {
        if (!canvas)
        {
            return;
        }
        get_canvas(canvas)->line_to(x, y);
    }

    AS_API void alphaskia_canvas_quadratic_curve_to(alphaskia_canvas_t canvas, float cpx, float cpy, float x, float y)
    {
        if (!canvas)
        {
            return;
        }
        get_canvas(canvas)->quadratic_curve_to(cpx, cpy, x, y);
    }

    AS_API void alphaskia_canvas_bezier_curve_to(alphaskia_canvas_t canvas, float cp1x, float cp1y, float cp2x, float cp2y, float x, float y)
    {
        if (!canvas)
        {
            return;
        }
        get_canvas(canvas)->bezier_curve_to(cp1x, cp1y, cp2x, cp2y, x, y);
    }

    AS_API void alphaskia_canvas_fill_circle(alphaskia_canvas_t canvas, float x, float y, float radius)
    {
        if (!canvas)
        {
            return;
        }
        get_canvas(canvas)->fill_circle(x, y, radius);
    }

    AS_API void alphaskia_canvas_stroke_circle(alphaskia_canvas_t canvas, float x, float y, float radius)
    {
        if (!canvas)
        {
            return;
        }
        get_canvas(canvas)->stroke_circle(x, y, radius);
    }

    AS_API void alphaskia_canvas_fill(alphaskia_canvas_t canvas)
    {
        if (!canvas)
        {
            return;
        }
        get_canvas(canvas)->fill();
    }

    AS_API void alphaskia_canvas_stroke(alphaskia_canvas_t canvas)
    {
        if (!canvas)
        {
            return;
        }
        get_canvas(canvas)->stroke();
    }

    AS_API void alphaskia_canvas_fill_text(alphaskia_canvas_t canvas, const char16_t *text, int text_length, alphaskia_text_style_t text_style, float font_size, float x, float y, alphaskia_text_align_t text_align, alphaskia_text_baseline_t baseline)
    {
        if (!canvas || !is_valid_text(text, text_length, text_style, font_size))
        {
            return;
        }

        AlphaSkiaTextStyle *skTextstyle = reinterpret_cast<AlphaSkiaTextStyle *>(text_style);
        get_canvas(canvas)->fill_text(text, text_length, *skTextstyle, font_size, x, y, text_align, baseline);
    }

    AS_API alphaskia_text_metrics_t alphaskia_canvas_measure_text(alphaskia_canvas_t canvas, const char16_t *text, int text_length, alphaskia_text_style_t text_style, float font_size, alphaskia_text_align_t text_align, alphaskia_text_baseline_t baseline)
    {
        if (!canvas || !is_valid_text(text, text_length, text_style, font_size))
        {
            return nullptr;
        }

        AlphaSkiaTextStyle *skTextstyle = reinterpret_cast<AlphaSkiaTextStyle *>(text_style);
        auto text_metrics = get_canvas(canvas)->measure_text(text, text_length, *skTextstyle, font_size, text_align, baseline);
        return reinterpret_cast<alphaskia_text_metrics_t>(text_metrics);
    }

    AS_API void alphaskia_canvas_begin_rotate(alphaskia_canvas_t canvas, float center_x, float center_y, float angle)
    {
        if (!canvas)
        {
            return;
        }
        get_canvas(canvas)->begin_rotate(center_x, center_y, angle);
    }

    AS_API void alphaskia_canvas_end_rotate(alphaskia_canvas_t canvas)
    {
        if (!canvas)
        {
            return;
        }
        get_canvas(canvas)->end_rotate();
    }
}
