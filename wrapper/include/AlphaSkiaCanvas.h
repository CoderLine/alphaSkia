#pragma once

#include "alphaskia.h"

#include "../../externals/skia/include/core/SkPath.h"
#include "../../externals/skia/include/core/SkCanvas.h"
#include "../../externals/skia/include/core/SkSurface.h"
#include "../../externals/skia/include/core/SkFont.h"
#include "../../externals/skia/include/core/SkTypeface.h"
#include "../../externals/skia/modules/skparagraph/include/FontCollection.h"
#include "../../externals/skia/modules/skparagraph/include/Paragraph.h"

#include <vector>

class AlphaSkiaTextStyle
{
public:
    AlphaSkiaTextStyle(
        int32_t weight,
        SkFontStyle::Slant slant,
        uint8_t family_name_count)
        : font_style_(weight, SkFontStyle::Width::kNormal_Width, slant)
    {
        family_names_.reserve(family_name_count);
    }

    const SkFontStyle &get_font_style() const
    {
        return font_style_;
    }

    std::vector<SkString> &get_family_names()
    {
        return family_names_;
    }

    const std::vector<SkString> &get_family_names() const
    {
        return family_names_;
    }

private:
    SkFontStyle font_style_;
    std::vector<SkString> family_names_;
};

class AlphaSkiaCanvas
{
public:
    AlphaSkiaCanvas();

    SkColor get_color() const;
    void set_color(SkColor color);

    float get_line_width() const;
    void set_line_width(float line_width);

    void begin_render(int32_t width, int32_t height, float render_scale);
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
    void draw_image(sk_sp<SkImage> image, float x, float y, float w, float h);

    void fill_text(const char16_t *text, int text_length, const AlphaSkiaTextStyle &textstyle, float font_size, float x, float y, alphaskia_text_align_t text_align, alphaskia_text_baseline_t baseline);
    void fill_text(const char16_t *text, int text_length, sk_sp<SkTypeface> typeface, float font_size, float x, float y, alphaskia_text_align_t text_align, alphaskia_text_baseline_t baseline);
    float measure_text(const char16_t *text, int text_length, const AlphaSkiaTextStyle &textstyle, float font_size);
    float measure_text(const char16_t *text, int text_length, sk_sp<SkTypeface> typeface, float font_size);
    void begin_rotate(float center_x, float center_y, float angle);
    void end_rotate();

private:
    SkPaint create_paint();
    std::unique_ptr<skia::textlayout::Paragraph> build_paragraph(const char16_t *text, int text_length, const AlphaSkiaTextStyle &textstyle, float font_size, alphaskia_text_align_t text_align);
    static float get_font_baseline(const SkFont &font, alphaskia_text_baseline_t baseline);

    SkColor color_;
    float line_width_;
    sk_sp<SkSurface> surface_;
    SkPath path_;

    sk_sp<skia::textlayout::FontCollection> font_collection_;
};