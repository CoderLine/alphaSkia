#include "../include/AlphaSkiaCanvas.h"

#include "../../externals/skia/include/core/SkData.h"
#include "../../externals/skia/include/core/SkPaint.h"
#include "../../externals/skia/include/core/SkTypeface.h"
#include "../../externals/skia/include/core/SkCanvas.h"
#include "../../externals/skia/include/core/SkSurface.h"
#include "../../externals/skia/include/core/SkPath.h"
#include "../../externals/skia/include/core/SkStream.h"
#include "../../externals/skia/include/core/SkFontMetrics.h"
#include "../../externals/skia/include/core/SkTextBlob.h"
#include "../../externals/skia/include/core/SkRefCnt.h"
#include "../../externals/skia/modules/skparagraph/include/FontCollection.h"
#include "../../externals/skia/modules/skparagraph/include/ParagraphBuilder.h"

#include <codecvt>
#include <locale>
#include <string>
#include <iostream>

AlphaSkiaCanvas::AlphaSkiaCanvas()
    : color_(SK_ColorWHITE), line_width_(1.0f)
{
    font_collection_ = sk_make_sp<skia::textlayout::FontCollection>();
    font_collection_->setDefaultFontManager(SkFontMgr::RefDefault());
}

SkPaint AlphaSkiaCanvas::create_paint()
{
    SkPaint paint;
    paint.setColor(color_);
    paint.setAntiAlias(true);
    paint.setDither(false);
    return paint;
}

SkColor AlphaSkiaCanvas::get_color() const
{
    return color_;
}
void AlphaSkiaCanvas::set_color(SkColor color)
{
    color_ = color;
}

float AlphaSkiaCanvas::get_line_width() const
{
    return line_width_;
}
void AlphaSkiaCanvas::set_line_width(float line_width)
{
    line_width_ = line_width;
}

void AlphaSkiaCanvas::begin_render(int32_t width, int32_t height, float render_scale)
{
    SkSurfaceProps props(0, kRGB_H_SkPixelGeometry);

    surface_ = SkSurfaces::Raster(SkImageInfo::Make(
                                      width * render_scale,
                                      height * render_scale,
                                      kN32_SkColorType,
                                      kPremul_SkAlphaType),
                                  &props);

    surface_->getCanvas()->scale(render_scale, render_scale);
    path_.transform(SkMatrix::Scale(1 / render_scale, 1 / render_scale));
}

sk_sp<SkImage> AlphaSkiaCanvas::end_render()
{
    return surface_->makeImageSnapshot();
}

void AlphaSkiaCanvas::fill_rect(float x, float y, float width, float height)
{
    SkPaint paint(create_paint());
    paint.setBlendMode(SkBlendMode::kSrcOver);
    paint.setStyle(SkPaint::kFill_Style);
    surface_->getCanvas()->drawRect(SkRect::MakeXYWH(x, y, width, height), paint);
}

void AlphaSkiaCanvas::stroke_rect(float x, float y, float width, float height)
{
    SkPaint paint(create_paint());
    paint.setStrokeWidth(line_width_);
    paint.setBlendMode(SkBlendMode::kSrcOver);
    paint.setStyle(SkPaint::kStroke_Style);
    surface_->getCanvas()->drawRect(SkRect::MakeXYWH(x, y, width, height), paint);
}

void AlphaSkiaCanvas::begin_path()
{
    path_.reset();
    path_.setFillType(SkPathFillType::kWinding);
}

void AlphaSkiaCanvas::close_path()
{
    path_.close();
}

void AlphaSkiaCanvas::move_to(float x, float y)
{
    path_.moveTo(x, y);
}

void AlphaSkiaCanvas::line_to(float x, float y)
{
    path_.lineTo(x, y);
}

void AlphaSkiaCanvas::quadratic_curve_to(float cpx, float cpy, float x, float y)
{
    path_.quadTo(cpx, cpy, x, y);
}

void AlphaSkiaCanvas::bezier_curve_to(float cp1x, float cp1y, float cp2x, float cp2y, float x, float y)
{
    path_.cubicTo(cp1x, cp1y, cp2x, cp2y, x, y);
}

void AlphaSkiaCanvas::fill_circle(float x, float y, float radius)
{
    begin_path();
    path_.addCircle(x, y, radius);
    close_path();
    fill();
}

void AlphaSkiaCanvas::stroke_circle(float x, float y, float radius)
{
    begin_path();
    path_.addCircle(x, y, radius);
    close_path();
    stroke();
}

void AlphaSkiaCanvas::fill()
{
    SkPaint paint(create_paint());
    paint.setStyle(SkPaint::kFill_Style);
    surface_->getCanvas()->drawPath(path_, paint);
    path_.reset();
}

void AlphaSkiaCanvas::stroke()
{
    SkPaint paint(create_paint());
    paint.setStrokeWidth(line_width_);
    paint.setStyle(SkPaint::kStroke_Style);
    surface_->getCanvas()->drawPath(path_, paint);
    path_.reset();
}

std::unique_ptr<skia::textlayout::Paragraph> AlphaSkiaCanvas::build_paragraph(const char16_t *text, int text_length, const AlphaSkiaTextStyle &textstyle, float font_size, alphaskia_text_align_t text_align)
{
    skia::textlayout::TextStyle style;

    SkPaint foregroundColor;
    foregroundColor.setColor(color_);
    style.setForegroundColor(foregroundColor);

    style.setFontFamilies(std::vector<SkString>(textstyle.get_family_names()));
    style.setFontStyle(textstyle.get_font_style());
    style.setFontSize(font_size);

    skia::textlayout::ParagraphStyle paraStyle;
    paraStyle.setTextHeightBehavior(skia::textlayout::TextHeightBehavior::kDisableAll);
    paraStyle.setTextStyle(style);

    switch (text_align)
    {
    case alphaskia_text_align_left:
        paraStyle.setTextAlign(skia::textlayout::TextAlign::kLeft);
        break;
    case alphaskia_text_align_center:
        paraStyle.setTextAlign(skia::textlayout::TextAlign::kCenter);
        break;
    case alphaskia_text_align_right:
        paraStyle.setTextAlign(skia::textlayout::TextAlign::kRight);
        break;
    }

    auto builder = skia::textlayout::ParagraphBuilder::make(paraStyle, font_collection_);
    builder->SetUnicode(SkUnicode::Make());
    builder->addText(text);

    return builder->Build();
}

void AlphaSkiaCanvas::fill_text(const char16_t *text, int text_length, const AlphaSkiaTextStyle &textstyle, float font_size, float x, float y, alphaskia_text_align_t text_align, alphaskia_text_baseline_t baseline)
{
    auto paragraph(build_paragraph(text, text_length, textstyle, font_size, text_align));

    // layout with enough space for our text to definitely fit
    const float layoutWidth = surface_->width() * 2;
    paragraph->layout(surface_->width() * 2);

    // NOTE: SkParagraph has no support for font/line specific baselines, first font is better than nothing
    y += get_font_baseline(paragraph->getFontAt(0), baseline);

    switch (text_align)
    {
    case alphaskia_text_align_left:
        // doesn't matter
        break;
    case alphaskia_text_align_center:
        x -= layoutWidth / 2;
        break;
    case alphaskia_text_align_right:
        // text is aligned at layoutWidth, shift it left
        x -= layoutWidth;
        break;
    }

    paragraph->paint(surface_->getCanvas(), x, y);
}

void AlphaSkiaCanvas::fill_text(const char16_t *text, int text_length, sk_sp<SkTypeface> typeface, float font_size, float x, float y, alphaskia_text_align_t text_align, alphaskia_text_baseline_t baseline)
{
    AlphaSkiaTextStyle textstyle(
        typeface->fontStyle().weight(),
        typeface->fontStyle().slant(),
        1);

    SkString familyName;
    typeface->getFamilyName(&familyName);
    textstyle.get_family_names().emplace_back(familyName);
    fill_text(text, text_length, textstyle, font_size, x, y, text_align, baseline);
}

float AlphaSkiaCanvas::measure_text(const char16_t *text, int text_length, const AlphaSkiaTextStyle &textstyle, float font_size)
{
    auto paragraph(build_paragraph(text, text_length, textstyle, font_size, alphaskia_text_align_t::alphaskia_text_align_left));

    paragraph->layout(10000);

    return static_cast<float>(paragraph->getMaxIntrinsicWidth());
}

float AlphaSkiaCanvas::measure_text(const char16_t *text, int text_length, sk_sp<SkTypeface> typeface, float font_size)
{
    AlphaSkiaTextStyle textstyle(
        typeface->fontStyle().weight(),
        typeface->fontStyle().slant(),
        1);

    SkString familyName;
    typeface->getFamilyName(&familyName);
    textstyle.get_family_names().emplace_back(familyName);

    return measure_text(text, text_length, textstyle, font_size);
}

void AlphaSkiaCanvas::begin_rotate(float center_x, float center_y, float angle)
{
    surface_->getCanvas()->save();
    surface_->getCanvas()->translate(center_x, center_y);
    surface_->getCanvas()->rotate(angle);
}

void AlphaSkiaCanvas::end_rotate()
{
    surface_->getCanvas()->restore();
}

void AlphaSkiaCanvas::draw_image(sk_sp<SkImage> image, float x, float y, float w, float h)
{
    SkSamplingOptions sampling;
    surface_->getCanvas()->drawImageRect(image, SkRect::MakeXYWH(x, y, w, h), sampling);
}

float float_ascent(const SkFontMetrics &metrics)
{
    // https://source.chromium.org/chromium/chromium/src/+/main:third_party/blink/renderer/platform/fonts/font_metrics.h;l=49?q=FloatAscent&ss=chromium%2Fchromium%2Fsrc
    // https://source.chromium.org/chromium/chromium/src/+/main:third_party/blink/renderer/platform/fonts/simple_font_data.cc;l=131;drc=5a2e12875a8fe207bfe6f0febc782b6297788b6d
    // https://source.chromium.org/chromium/chromium/src/+/main:third_party/blink/renderer/platform/fonts/font_metrics.cc;l=112;drc=5a2e12875a8fe207bfe6f0febc782b6297788b6d;bpv=1;bpt=1
    return SkScalarRoundToScalar(-metrics.fAscent);
}

float float_descent(const SkFontMetrics &metrics)
{
    // https://source.chromium.org/chromium/chromium/src/+/main:third_party/blink/renderer/platform/fonts/font_metrics.cc;l=112;drc=5a2e12875a8fe207bfe6f0febc782b6297788b6d;bpv=1;bpt=1
    return SkScalarRoundToScalar(metrics.fDescent);
}

std::pair<int16_t, int16_t> typo_ascender_and_descender(SkTypeface *typeface)
{
    // https://source.chromium.org/chromium/chromium/src/+/main:third_party/blink/renderer/platform/fonts/simple_font_data.cc;l=388;drc=5a2e12875a8fe207bfe6f0febc782b6297788b6d?q=TypoAscenderAndDescender&sq=&ss=chromium%2Fchromium%2Fsrc
    uint8_t buffer[4];
    size_t size = typeface->getTableData(SkSetFourByteTag('O', 'S', '/', '2'), 68,
                                         sizeof(buffer), buffer);
    if (size == sizeof(buffer))
    {
        return std::make_pair(
            (int16_t)((buffer[0] << 8) | buffer[1]),
            -(int16_t)((buffer[2] << 8) | buffer[3]));
    }

    return std::make_pair(0, 0);
}

const uint32_t layoutUnitFractionalBits_ = 6;
const int fixedPointDenominator_ = 1 << layoutUnitFractionalBits_;

int float_to_layout_unit(float value)
{
    // https://source.chromium.org/chromium/chromium/src/+/main:third_party/blink/renderer/platform/geometry/layout_unit.h;l=147;drc=5a2e12875a8fe207bfe6f0febc782b6297788b6d;bpv=1;bpt=1?q=FromFloatRound&ss=chromium%2Fchromium%2Fsrc
    return static_cast<int>(roundf(value * fixedPointDenominator_));
}

float layout_unit_to_float(int value)
{
    // https://source.chromium.org/chromium/chromium/src/+/main:third_party/blink/renderer/platform/geometry/layout_unit.h;l=147;drc=5a2e12875a8fe207bfe6f0febc782b6297788b6d;bpv=1;bpt=1?q=FromFloatRound&sq=&ss=chromium%2Fchromium%2Fsrc    return static_cast<int>(roundf(value * kFixedPointDenominator))
    return static_cast<float>(value) / fixedPointDenominator_;
}

bool try_set_normalized_typo_ascent_and_descent(float em_height, float typo_ascent, float typo_descent, int &ascent, int &descent)
{
    // https://source.chromium.org/chromium/chromium/src/+/main:third_party/blink/renderer/platform/fonts/simple_font_data.cc;l=422;drc=5a2e12875a8fe207bfe6f0febc782b6297788b6d;bpv=1;bpt=1?q=NormalizedTypoAscentAndDescent&ss=chromium%2Fchromium%2Fsrc
    const float height = typo_ascent + typo_descent;
    if (height <= 0 || typo_ascent < 0 || typo_descent > height)
    {
        return false;
    }

    ascent = float_to_layout_unit(typo_ascent * em_height / height);
    descent = float_to_layout_unit(em_height) - ascent;
    return true;
}

void normalized_typo_ascent_and_descent(const SkFont &font, int &ascent, int &descent)
{
    // https://source.chromium.org/chromium/chromium/src/+/main:third_party/blink/renderer/platform/fonts/simple_font_data.cc;l=366;drc=5a2e12875a8fe207bfe6f0febc782b6297788b6d;bpv=1;bpt=1?q=NormalizedTypoAscentAndDescent&ss=chromium%2Fchromium%2Fsrc
    SkTypeface *typeface = font.getTypeface();
    auto [typo_ascender, typo_descender] = typo_ascender_and_descender(typeface);

    if (typo_ascender > 0 &&
        try_set_normalized_typo_ascent_and_descent(font.getSize(), typo_ascender, typo_descender, ascent, descent))
    {
        return;
    }

    // As the last resort, compute em height metrics from our ascent/descent.
    SkFontMetrics metrics;
    font.getMetrics(&metrics);
    if (try_set_normalized_typo_ascent_and_descent(font.getSize(), float_ascent(metrics), float_descent(metrics), ascent, descent))
    {
        return;
    }
}

float AlphaSkiaCanvas::get_font_baseline(const SkFont &font, alphaskia_text_baseline_t baseline)
{
    // https://github.com/chromium/chromium/blob/99314be8152e688bafbbf9a615536bdbb289ea87/third_party/blink/renderer/core/html/canvas/text_metrics.cc#L14
    SkFontMetrics metrics;
    font.getMetrics(&metrics);

    float baselineOffset = 0;
    int ascent(0);
    int descent(0);

    switch (baseline)
    {
    case alphaskia_text_baseline_alphabetic: // kAlphabeticTextBaseline
        baselineOffset = 0;
        break;
    case alphaskia_text_baseline_top: // kHangingTextBaseline
#define kHangingAsPercentOfAscent 80
        baselineOffset = float_ascent(metrics) * kHangingAsPercentOfAscent / 100.0f;
        break;
    case alphaskia_text_baseline_middle: // kMiddleTextBaseline
    {
        normalized_typo_ascent_and_descent(font, ascent, descent);
        auto middle = (layout_unit_to_float(ascent) - layout_unit_to_float(descent)) / 2.0f;
        baselineOffset = middle;
        break;
    }
    case alphaskia_text_baseline_bottom: // kBottomTextBaseline
        normalized_typo_ascent_and_descent(font, ascent, descent);
        baselineOffset = -layout_unit_to_float(descent);
        break;
    }

    // SkParagraph defines its baseline() as (fLeading / 2 - fAscent)
    // see: Run.h -> InternalLineMetrics::baseline()
    // we reset this here
    const float skParagraphBaseline = metrics.fLeading / 2 + float_ascent(metrics);
    baselineOffset -= skParagraphBaseline;

    return baselineOffset;
}