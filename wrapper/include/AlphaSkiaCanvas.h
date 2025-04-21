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

class AlphaSkiaTextMetrics
{
public:
    AlphaSkiaTextMetrics(
        float width,
        float actual_bounding_box_left,
        float actual_bounding_box_right,
        float font_bounding_box_ascent,
        float font_bounding_box_descent,
        float em_height_ascent,
        float em_height_descent,
        float hanging_baseline,
        float alphabetic_baseline,
        float ideographic_baseline)
        : width_(width), actual_bounding_box_left_(actual_bounding_box_left), actual_bounding_box_right_(actual_bounding_box_right), font_bounding_box_ascent_(font_bounding_box_ascent), font_bounding_box_descent_(font_bounding_box_descent), em_height_ascent_(em_height_ascent), em_height_descent_(em_height_descent), hanging_baseline_(hanging_baseline), alphabetic_baseline_(alphabetic_baseline), ideographic_baseline_(ideographic_baseline)
    {
    }

    float get_width() const { return width_; }
    float get_actual_bounding_box_left() const { return actual_bounding_box_left_; }
    float get_actual_bounding_box_right() const { return actual_bounding_box_right_; }
    float get_font_bounding_box_ascent() const { return font_bounding_box_ascent_; }
    float get_font_bounding_box_descent() const { return font_bounding_box_descent_; }
    float get_em_height_ascent() const { return em_height_ascent_; }
    float get_em_height_descent() const { return em_height_descent_; }
    float get_hanging_baseline() const { return hanging_baseline_; }
    float get_alphabetic_baseline() const { return alphabetic_baseline_; }
    float get_ideographic_baseline() const { return ideographic_baseline_; }

private:
    float width_;
    float actual_bounding_box_left_;
    float actual_bounding_box_right_;
    float font_bounding_box_ascent_;
    float font_bounding_box_descent_;
    float em_height_ascent_;
    float em_height_descent_;
    float hanging_baseline_;
    float alphabetic_baseline_;
    float ideographic_baseline_;
};

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

    void fill_text(const char16_t *text, int text_length, const AlphaSkiaTextStyle &text_style, float font_size, float x, float y, alphaskia_text_align_t text_align, alphaskia_text_baseline_t baseline);
    AlphaSkiaTextMetrics* measure_text(const char16_t *text, int text_length, const AlphaSkiaTextStyle &text_style, float font_size, alphaskia_text_align_t text_align, alphaskia_text_baseline_t baseline);
    void begin_rotate(float center_x, float center_y, float angle);
    void end_rotate();

private:
    SkPaint create_paint();
    std::unique_ptr<skia::textlayout::Paragraph> build_paragraph(const char16_t *text, int text_length, const AlphaSkiaTextStyle &text_style, float font_size, alphaskia_text_align_t text_align);
    static float get_font_baseline(const SkFont &font, alphaskia_text_baseline_t baseline);

    SkColor color_;
    float line_width_;
    sk_sp<SkSurface> surface_;
    SkPath path_;

    sk_sp<skia::textlayout::FontCollection> font_collection_;
};