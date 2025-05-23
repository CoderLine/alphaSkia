#include "../include/AlphaSkiaCanvas.h"
#include "../include/SkFontMgr_alphaskia.h"

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
#include "../../externals/skia/modules/skparagraph/src/ParagraphImpl.h"

#include <codecvt>
#include <locale>
#include <string>
#include <iostream>

#define kHangingAsPercentOfAscent 80

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

bool try_set_normalized_typo_ascent_and_descent(float em_height, float typo_ascent, float typo_descent, int &ascent, int &descent)
{
    // https://source.chromium.org/chromium/chromium/src/+/main:third_party/blink/renderer/platform/fonts/simple_font_data.cc;l=422;drc=5a2e12875a8fe207bfe6f0febc782b6297788b6d;bpv=1;bpt=1?q=NormalizedTypoAscentAndDescent&ss=chromium%2Fchromium%2Fsrc
    const float height = typo_ascent + typo_descent;
    if (height <= 0 || typo_ascent < 0 || typo_descent > height)
    {
        return false;
    }

    ascent = typo_ascent * em_height / height;
    descent = em_height - ascent;
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

AlphaSkiaCanvas::AlphaSkiaCanvas()
    : color_(SK_ColorWHITE), line_width_(1.0f)
{
    font_collection_ = sk_make_sp<skia::textlayout::FontCollection>();
    font_collection_->setDefaultFontManager(SkFontMgr_AlphaSkia::instance());
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
                                      std::max(width, 1) * render_scale,
                                      std::max(height, 1) * render_scale,
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

std::unique_ptr<skia::textlayout::Paragraph> AlphaSkiaCanvas::build_paragraph(const char16_t *text, int text_length, const AlphaSkiaTextStyle &text_style, float font_size, alphaskia_text_align_t text_align)
{
    skia::textlayout::TextStyle style;

    SkPaint foregroundColor;
    foregroundColor.setColor(color_);
    style.setForegroundColor(foregroundColor);

    style.setFontFamilies(std::vector<SkString>(text_style.get_family_names()));
    style.setFontStyle(text_style.get_font_style());
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
    builder->addText(text);

    return builder->Build();
}

void AlphaSkiaCanvas::fill_text(const char16_t *text, int text_length, const AlphaSkiaTextStyle &text_style, float font_size, float x, float y, alphaskia_text_align_t text_align, alphaskia_text_baseline_t baseline)
{
    auto paragraph(build_paragraph(text, text_length, text_style, font_size, text_align));

    // layout with enough space for our text to definitely fit
    const float layoutWidth = surface_->width() * 2;
    paragraph->layout(surface_->width() * 2);

    // NOTE: SkParagraph has no support for font/line specific baselines, first font is better than nothing
    y += get_font_baseline(paragraph->getFontAt(0), baseline, true);

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

AlphaSkiaTextMetrics *AlphaSkiaCanvas::measure_text(const char16_t *text, int text_length, const AlphaSkiaTextStyle &text_style, float font_size, alphaskia_text_align_t text_align, alphaskia_text_baseline_t baseline)
{
    auto paragraph(build_paragraph(text, text_length, text_style, font_size, alphaskia_text_align_t::alphaskia_text_align_left));
    const float layoutWidth = 10000;
    paragraph->layout(layoutWidth);

    // https://source.chromium.org/chromium/chromium/src/+/main:third_party/blink/renderer/modules/canvas/canvas2d/base_rendering_context_2d.cc;l=1290
    // https://source.chromium.org/chromium/chromium/src/+/main:third_party/blink/renderer/core/html/canvas/text_metrics.cc

    // the original HTML5 canvas doesn't really support newlines, we simply calculate everything with the
    // first line

    skia::textlayout::ParagraphImpl *paragraphImpl = reinterpret_cast<skia::textlayout::ParagraphImpl *>(paragraph.get());

    float width = static_cast<float>(paragraph->getMaxIntrinsicWidth());

    auto text_align_dx_ = 0.0f;
    switch (text_align)
    {
    case alphaskia_text_align_left:
        // doesn't matter
        break;
    case alphaskia_text_align_center:
        text_align_dx_ = width / 2.0f;
        text_align_dx_ -= layoutWidth / 2;
        break;
    case alphaskia_text_align_right:
        // text is aligned at layoutWidth, shift it left
        text_align_dx_ = width;
        text_align_dx_ -= layoutWidth;
        break;
    }

    SkFont font = paragraph->getFontAt(0);
    SkFontMetrics font_metrics;
    font.getMetrics(&font_metrics);

    const float ascent = float_ascent(font_metrics);
    const float descent = float_descent(font_metrics);
    float baseline_y = get_font_baseline(font, baseline, false);
    float actual_bounding_box_ascent = 0;
    float actual_bounding_box_descent = 0;
    float lineOffsetX = 0;
    float actual_bounding_box_left = -lineOffsetX + text_align_dx_;
    float actual_bounding_box_right = actual_bounding_box_left;

    if (paragraphImpl->lines().size() > 0)
    {

        auto &line0 = paragraphImpl->lines()[0];
        lineOffsetX = line0.offset().fX;

        line0.iterateThroughVisualRuns(
            false,
            [&](const skia::textlayout::Run *run, SkScalar runOffsetInLine, skia::textlayout::TextRange textRange, SkScalar *runWidthInLine)
            {
                if (run->size() > 0)
                {
                    std::vector<SkRect> bounds;
                    bounds.resize(run->size());
                    run->font().getBounds(&run->glyphs()[0], run->size(), bounds.data(), nullptr);

                    for (size_t i = 0; i < run->size(); i++)
                    {
                        if (bounds[i].fTop < actual_bounding_box_ascent)
                        {
                            actual_bounding_box_ascent = bounds[i].fTop;
                        }

                        if (bounds[i].fBottom > actual_bounding_box_descent)
                        {
                            actual_bounding_box_descent = bounds[i].fBottom;
                        }
                    }
                }
                return true;
            });
        actual_bounding_box_ascent = std::abs(actual_bounding_box_ascent);
        actual_bounding_box_left = -lineOffsetX + text_align_dx_;
        actual_bounding_box_right = (lineOffsetX + line0.width()) - text_align_dx_;
    }

    float font_bounding_box_ascent = ascent - baseline_y;
    float font_bounding_box_descent = descent + baseline_y;

    int normalizedAscent = 0;
    int normalizedDescent = 0;
    normalized_typo_ascent_and_descent(font, normalizedAscent, normalizedDescent);
    float em_height_ascent = normalizedAscent - baseline_y;
    float em_height_descent = normalizedDescent + baseline_y;
    // https://source.chromium.org/chromium/chromium/src/+/main:third_party/blink/renderer/core/html/canvas/text_metrics.cc;l=174;drc=1d737975521c1d4191937c2c659bd78d9f1681f4;bpv=0;bpt=1
    float hanging_baseline = ascent * kHangingAsPercentOfAscent / 100.0f - baseline_y;

    // TODO: Read baselines value from OpenType Table. https://source.chromium.org/chromium/chromium/src/+/main:third_party/blink/renderer/platform/fonts/simple_font_data.cc;l=229;drc=73bcf25e4b0c06b1a86422a2fc30023c7b850f33;bpv=1;bpt=1
    float alphabetic_baseline = -baseline_y;
    float ideographic_baseline = -descent - baseline_y;

    return new AlphaSkiaTextMetrics(
        width,
        actual_bounding_box_left,
        actual_bounding_box_right,
        font_bounding_box_ascent,
        font_bounding_box_descent,
        actual_bounding_box_ascent,
        actual_bounding_box_descent,
        em_height_ascent,
        em_height_descent,
        hanging_baseline,
        alphabetic_baseline,
        ideographic_baseline);
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

float AlphaSkiaCanvas::get_font_baseline(const SkFont &font, alphaskia_text_baseline_t baseline, bool correctSkParagraphBaseline)
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
        baselineOffset = float_ascent(metrics) * kHangingAsPercentOfAscent / 100.0f;
        break;
    case alphaskia_text_baseline_middle: // kMiddleTextBaseline
    {
        normalized_typo_ascent_and_descent(font, ascent, descent);
        auto middle = (ascent - descent) / 2.0f;
        baselineOffset = middle;
        break;
    }
    case alphaskia_text_baseline_bottom: // kBottomTextBaseline
        normalized_typo_ascent_and_descent(font, ascent, descent);
        baselineOffset = -descent;
        break;
    }

    // SkParagraph defines its baseline() as (fLeading / 2 - fAscent)
    // see: Run.h -> InternalLineMetrics::baseline()
    // we reset this here
    if (correctSkParagraphBaseline)
    {
        const float skParagraphBaseline = metrics.fLeading / 2 + float_ascent(metrics);
        baselineOffset -= skParagraphBaseline;
    }

    return baselineOffset;
}