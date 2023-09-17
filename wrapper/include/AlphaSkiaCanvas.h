#pragma once

#include "libAlphaSkia.h"

#include "../../externals/skia/include/core/SkPath.h"
#include "../../externals/skia/include/core/SkCanvas.h"
#include "../../externals/skia/include/core/SkSurface.h"
#include "../../externals/skia/include/core/SkFont.h"
#include "../../externals/skia/include/core/SkTypeface.h"

class AlphaSkiaCanvas
{
public:
    AlphaSkiaCanvas();

    SkColor get_color() const;
    void set_color(SkColor color);

    float get_line_width() const;
    void set_line_width(float line_width);

    void begin_render(int32_t width, int32_t height);
    sk_sp<SkImage> end_render();

    void fill_rect(float x, float y, float width, float height);
    void stroke_rect(float x, float y, float width, float height);
    void begin_path();
    void close_path();
    void move_to(float x, float y);
    void line_to(float x, float y);
    void quadratic_curve_to(float cpx, float cpy, float x, float y);
    void bezier_curve_to(float cp1x, float cp1y, float cp2x, float cp2y, float x, float y);
    void fill_circle(float x, float y, float radius);
    void stroke_circle(float x, float y, float radius);
    void fill();
    void stroke();
    void draw_image(sk_sp<SkImage> image, float x, float y);

    void fill_text(wchar_t *text, sk_sp<SkTypeface> type_face, float font_size, float x, float y, alphaskia_text_align_t text_align, alphaskia_text_baseline_t baseline);

    float measure_text(wchar_t *text, sk_sp<SkTypeface> type_face, float font_size);
    void begin_rotate(float center_x, float center_y, float angle);
    void end_rotate();

private:
    SkPaint create_paint();

    std::string convert_to_utf8(wchar_t *text);

    SkColor color_;
    float line_width_;
    sk_sp<SkSurface> surface_;
    SkPath path_;

    // Chromium adopted text shaping and blob creation
    static const uint32_t layoutUnitFractionalBits_;
    static const int32_t fixedPointDenominator_;
    void text_run(wchar_t *text,
                  SkFont& font,
                  sk_sp<SkTextBlob> &realBlob,
                  float &width);

    static int float_to_layout_unit(float value);
    static float layout_unit_to_float(int value);
    static bool try_set_normalized_typo_ascent_and_descent(float em_height, float typo_ascent, float typo_descent, int &ascent, int &descent);
    static void normalized_typo_ascent_and_descent(const SkFont &font, int &ascent, int &descent);
    static float get_font_baseline(const SkFont &font, alphaskia_text_baseline_t baseline);
};