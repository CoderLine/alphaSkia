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
#include "../../externals/skia/third_party/externals/harfbuzz/src/hb.h"
#include "../../externals/skia/third_party/externals/harfbuzz/src/hb-ot.h"

#include <algorithm>
#include <codecvt>
#include <locale>
#include <optional>
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

struct TextRun
{
    SkFont font;
    std::vector<SkGlyphID> glyphs;
    std::vector<SkPoint> positions;
};

// Baselines from the OpenType BASE table in pixels, positive values are above the alphabetic baseline.
struct OpenTypeBaselines
{
    std::optional<float> alphabetic;
    std::optional<float> hanging;
    std::optional<float> ideographic;
};

hb_blob_t *get_typeface_table(hb_face_t *, hb_tag_t tag, void *user_data)
{
    auto data = reinterpret_cast<SkTypeface *>(user_data)->copyTableData(tag);
    if (!data)
    {
        return nullptr;
    }
    SkData *raw_data = data.release();
    return hb_blob_create(reinterpret_cast<const char *>(raw_data->data()), static_cast<unsigned int>(raw_data->size()),
                          HB_MEMORY_MODE_READONLY, raw_data, [](void *context)
                          { SkSafeUnref(reinterpret_cast<SkData *>(context)); });
}

OpenTypeBaselines read_open_type_baselines(const SkFont &font)
{
    // https://source.chromium.org/chromium/chromium/src/+/main:third_party/blink/renderer/platform/fonts/opentype/open_type_baseline_metrics.cc
    OpenTypeBaselines baselines;
    SkTypeface *typeface = font.getTypeface();
    if (typeface->getTableSize(SkSetFourByteTag('B', 'A', 'S', 'E')) == 0)
    {
        return baselines;
    }

    hb_face_t *face = hb_face_create_for_tables(get_typeface_table, typeface, nullptr);
    hb_face_set_upem(face, typeface->getUnitsPerEm());
    hb_font_t *hb_font = hb_font_create(face);
    // 16.16 fixed point scaled to the font size, like Chromium's HarfBuzzFace::GetScaledFont
    const int scale = static_cast<int>(font.getSize() * (1 << 16));
    hb_font_set_scale(hb_font, scale, scale);

    auto read = [hb_font](hb_ot_layout_baseline_tag_t tag) -> std::optional<float>
    {
        hb_position_t position;
        if (hb_ot_layout_get_baseline(hb_font, tag, HB_DIRECTION_LTR, HB_OT_TAG_DEFAULT_SCRIPT, HB_OT_TAG_DEFAULT_LANGUAGE, &position))
        {
            return position / static_cast<float>(1 << 16);
        }
        return std::nullopt;
    };
    baselines.alphabetic = read(HB_OT_LAYOUT_BASELINE_TAG_ROMAN);
    baselines.hanging = read(HB_OT_LAYOUT_BASELINE_TAG_HANGING);
    baselines.ideographic = read(HB_OT_LAYOUT_BASELINE_TAG_IDEO_EMBOX_BOTTOM_OR_LEFT);

    hb_font_destroy(hb_font);
    hb_face_destroy(face);
    return baselines;
}

// Text shaped into a single line, positioned relative to the alphabetic baseline at the start of the text.
struct TextLayout
{
    // The first available font of the text style, all baselines and font metrics are taken from it.
    SkFont primary_font;
    OpenTypeBaselines primary_baselines;
    float width = 0;
    SkRect glyph_bounds = SkRect::MakeEmpty();
    std::vector<TextRun> runs;
};

SkFont make_shaping_font(sk_sp<SkTypeface> typeface, const AlphaSkiaTextStyle &text_style, float font_size)
{
    // Same settings as SkParagraph's OneLineShaper uses for the shaped runs, so metrics match the drawn glyphs.
    SkFont font(std::move(typeface), font_size);
    font.setEdging(SkFont::Edging::kAntiAlias);
    font.setHinting(SkFontHinting::kSlight);
    font.setSubpixel(true);

    const int wanted_weight = text_style.get_font_style().weight();
    const bool fake_bold = wanted_weight >= SkFontStyle::kSemiBold_Weight &&
                           wanted_weight - font.getTypeface()->fontStyle().weight() >= 200;
    const bool fake_italic = text_style.get_font_style().slant() == SkFontStyle::kItalic_Slant &&
                             font.getTypeface()->fontStyle().slant() != SkFontStyle::kItalic_Slant;
    font.setEmbolden(fake_bold);
    font.setSkewX(fake_italic ? -SK_Scalar1 / 4 : 0);
    return font;
}

TextLayout layout_text(const sk_sp<skia::textlayout::FontCollection> &font_collection, const char16_t *text, int text_length, const AlphaSkiaTextStyle &text_style, float font_size)
{
    // https://html.spec.whatwg.org/multipage/canvas.html#text-preparation-algorithm
    // Like Chromium's IsNormalizedCanvasSpaceCharacter, U+000B is replaced as well.
    // Deviation from Chromium: U+2028 is replaced because SkParagraph would start a new line at it. Chromium shapes it
    // like any other character, typically as .notdef glyph of the primary font.
    std::u16string prepared_text(text, static_cast<size_t>(text_length));
    for (auto &c : prepared_text)
    {
        switch (c)
        {
        case u'\t':
        case u'\n':
        case u'\v':
        case u'\f':
        case u'\r':
        case u'\u2028':
            c = u' ';
            break;
        }
    }

    skia::textlayout::TextStyle style;
    style.setFontFamilies(text_style.get_family_names());
    style.setFontStyle(text_style.get_font_style());
    style.setFontSize(font_size);

    skia::textlayout::ParagraphStyle paragraph_style;
    paragraph_style.setTextStyle(style);
    paragraph_style.setTextAlign(skia::textlayout::TextAlign::kLeft);
    paragraph_style.setApplyRoundingHack(false);

    auto builder = skia::textlayout::ParagraphBuilder::make(paragraph_style, font_collection);
    builder->addText(prepared_text);
    auto paragraph = builder->Build();
    paragraph->layout(SK_ScalarInfinity);

    TextLayout layout;

    // https://drafts.csswg.org/css-fonts-4/#first-available-font
    // Unavailable families are skipped, SkParagraph shapes with the same list.
    auto typefaces = font_collection->findTypefaces(text_style.get_family_names(), text_style.get_font_style());
    layout.primary_font = make_shaping_font(typefaces.empty() ? nullptr : typefaces.front(), text_style, font_size);
    layout.primary_baselines = read_open_type_baselines(layout.primary_font);

    layout.width = paragraph->getMaxIntrinsicWidth();

    // Only the glyph runs are taken from the paragraph. Its line box would place the baseline below the
    // largest ascent of all fonts, while canvas text uses the baseline of the primary font for all glyphs.
    paragraph->visit(
        [&](int, const skia::textlayout::Paragraph::VisitorInfo *info)
        {
            if (info == nullptr || info->count == 0)
            {
                return;
            }

            TextRun run{info->font, std::vector<SkGlyphID>(info->glyphs, info->glyphs + info->count), {}};
            run.positions.reserve(info->count);
            for (int i = 0; i < info->count; i++)
            {
                // info->origin.fY is SkParagraph's line baseline, glyph positions are relative to it
                run.positions.push_back(SkPoint::Make(info->origin.fX + info->positions[i].fX, info->positions[i].fY));
            }

            std::vector<SkRect> bounds(info->count);
            run.font.getBounds(run.glyphs.data(), info->count, bounds.data(), nullptr);
            for (int i = 0; i < info->count; i++)
            {
                if (!bounds[i].isEmpty())
                {
                    layout.glyph_bounds.join(bounds[i].makeOffset(run.positions[i]));
                }
            }

            layout.runs.push_back(std::move(run));
        });

    if (layout.glyph_bounds.isEmpty())
    {
        layout.glyph_bounds = SkRect::MakeEmpty();
    }

    return layout;
}

float get_text_align_dx(alphaskia_text_align_t text_align, float width)
{
    switch (text_align)
    {
    case alphaskia_text_align_center:
        return width / 2;
    case alphaskia_text_align_right:
        return width;
    case alphaskia_text_align_left:
    default:
        return 0;
    }
}

float get_font_baseline(const SkFont &font, const OpenTypeBaselines &open_type_baselines, alphaskia_text_baseline_t baseline)
{
    // https://source.chromium.org/chromium/chromium/src/+/main:third_party/blink/renderer/core/html/canvas/text_metrics.cc
    // TextMetrics::GetFontBaseline
    SkFontMetrics metrics;
    font.getMetrics(&metrics);

    int ascent(0);
    int descent(0);

    switch (baseline)
    {
    case alphaskia_text_baseline_alphabetic: // kAlphabeticTextBaseline
        return open_type_baselines.alphabetic.value_or(0);
    case alphaskia_text_baseline_top: // kHangingTextBaseline
        return open_type_baselines.hanging.value_or(float_ascent(metrics) * kHangingAsPercentOfAscent / 100.0f);
    case alphaskia_text_baseline_middle: // kMiddleTextBaseline
        normalized_typo_ascent_and_descent(font, ascent, descent);
        return (ascent - descent) / 2.0f;
    case alphaskia_text_baseline_bottom: // kBottomTextBaseline
        normalized_typo_ascent_and_descent(font, ascent, descent);
        return -descent;
    }

    return 0;
}

void AlphaSkiaCanvas::fill_text(const char16_t *text, int text_length, const AlphaSkiaTextStyle &text_style, float font_size, float x, float y, alphaskia_text_align_t text_align, alphaskia_text_baseline_t baseline)
{
    auto layout(layout_text(font_collection_, text, text_length, text_style, font_size));
    if (layout.runs.empty())
    {
        return;
    }

    // https://source.chromium.org/chromium/chromium/src/+/main:third_party/blink/renderer/modules/canvas/canvas2d/base_rendering_context_2d.cc
    // BaseRenderingContext2D::DrawTextInternal
    x -= get_text_align_dx(text_align, layout.width);
    y += get_font_baseline(layout.primary_font, layout.primary_baselines, baseline);

    SkTextBlobBuilder builder;
    for (const auto &run : layout.runs)
    {
        const auto &buffer = builder.allocRunPos(run.font, static_cast<int>(run.glyphs.size()));
        std::copy(run.glyphs.begin(), run.glyphs.end(), buffer.glyphs);
        std::copy(run.positions.begin(), run.positions.end(), buffer.points());
    }

    SkPaint paint;
    paint.setColor(color_);
    surface_->getCanvas()->drawTextBlob(builder.make(), x, y, paint);
}

AlphaSkiaTextMetrics *AlphaSkiaCanvas::measure_text(const char16_t *text, int text_length, const AlphaSkiaTextStyle &text_style, float font_size, alphaskia_text_align_t text_align, alphaskia_text_baseline_t baseline)
{
    auto layout(layout_text(font_collection_, text, text_length, text_style, font_size));

    // https://source.chromium.org/chromium/chromium/src/+/main:third_party/blink/renderer/core/html/canvas/text_metrics.cc
    // TextMetrics::InitFromShapedText and TextMetrics::PopulateVerticalMetrics
    const float width = layout.width;
    const float text_align_dx = get_text_align_dx(text_align, width);
    const SkRect &glyph_bounds = layout.glyph_bounds;

    SkFontMetrics font_metrics;
    layout.primary_font.getMetrics(&font_metrics);
    const float ascent = float_ascent(font_metrics);
    const float descent = float_descent(font_metrics);
    const float baseline_y = get_font_baseline(layout.primary_font, layout.primary_baselines, baseline);

    int normalized_ascent = 0;
    int normalized_descent = 0;
    normalized_typo_ascent_and_descent(layout.primary_font, normalized_ascent, normalized_descent);

    // Deviation from Chromium: the reported width is rounded to 1/100 px like SkParagraph's applyRoundingHack, which
    // layouts built on measured widths (e.g. alphaTab references) depend on. Chromium reports the unrounded advance.
    // Alignment, here and in fill_text, uses the unrounded width; the difference is at most 0.005 px.
    const float reported_width = SkScalarRoundToScalar(width * 100.0) / 100.0;

    const OpenTypeBaselines &open_type_baselines = layout.primary_baselines;
    return new AlphaSkiaTextMetrics(
        reported_width,
        -glyph_bounds.left() + text_align_dx,
        glyph_bounds.right() - text_align_dx,
        ascent - baseline_y,
        descent + baseline_y,
        -glyph_bounds.top() - baseline_y,
        glyph_bounds.bottom() + baseline_y,
        normalized_ascent - baseline_y,
        normalized_descent + baseline_y,
        open_type_baselines.hanging.value_or(ascent * kHangingAsPercentOfAscent / 100.0f) - baseline_y,
        open_type_baselines.alphabetic.value_or(0) - baseline_y,
        open_type_baselines.ideographic.value_or(-descent) - baseline_y);
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
