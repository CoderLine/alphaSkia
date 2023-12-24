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

#include "../../externals/skia/third_party/externals/harfbuzz/src/hb.h"
#include "../../externals/skia/third_party/externals/harfbuzz/src/hb-ot.h"

#include <codecvt>
#include <locale>
#include <string>
#include <iostream>

AlphaSkiaCanvas::AlphaSkiaCanvas()
    : color_(SK_ColorWHITE), line_width_(1.0f)
{
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
    surface_ = SkSurfaces::Raster(SkImageInfo::Make(
        width * render_scale,
        height * render_scale,
        kN32_SkColorType,
        kPremul_SkAlphaType));

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

void AlphaSkiaCanvas::fill_text(const char16_t *text, int text_length, sk_sp<SkTypeface> type_face, float font_size, float x, float y, alphaskia_text_align_t text_align, alphaskia_text_baseline_t baseline)
{
    sk_sp<SkTextBlob> realBlob;
    SkFont font(type_face, font_size);

    float width(0);
    text_run(text, text_length, font, realBlob, width);

    switch (text_align)
    {
    case alphaskia_text_align_left:
        break;
    case alphaskia_text_align_center:
        x -= width / 2;
        break;
    case alphaskia_text_align_right:
        x -= width;
        break;
    }

    y += get_font_baseline(font, baseline);

    if (realBlob)
    {
        SkPaint paint(create_paint());
        paint.setStyle(SkPaint::kFill_Style);
        surface_->getCanvas()->drawTextBlob(realBlob, x,
                                            y,
                                            paint);
    }
}

float AlphaSkiaCanvas::measure_text(const char16_t *text, int text_length, sk_sp<SkTypeface> type_face, float font_size)
{
    sk_sp<SkTextBlob> realBlob;
    SkFont font(type_face, font_size);
    float width(0);
    text_run(text, text_length, font, realBlob, width);
    return width;
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

using HBBlob = std::unique_ptr<hb_blob_t, SkFunctionObject<hb_blob_destroy>>;
using HBFace = std::unique_ptr<hb_face_t, SkFunctionObject<hb_face_destroy>>;
using HBFont = std::unique_ptr<hb_font_t, SkFunctionObject<hb_font_destroy>>;
using HBBuffer = std::unique_ptr<hb_buffer_t, SkFunctionObject<hb_buffer_destroy>>;

const int SkiaToHarfBuzzFontSize = 1 << 16;
const float HarfBuzzToSkiaFontSize = 1.0f / SkiaToHarfBuzzFontSize;

hb_blob_t* skhb_get_table(hb_face_t* face, hb_tag_t tag, void* user_data) 
{
    SkTypeface& typeface = *reinterpret_cast<SkTypeface*>(user_data);

    auto data = typeface.copyTableData(tag);
    if (!data) 
    {
        return nullptr;
    }
    SkData* rawData = data.release();
    return hb_blob_create(reinterpret_cast<char*>(rawData->writable_data()), rawData->size(),
                          HB_MEMORY_MODE_READONLY, rawData, [](void* ctx) {
                              SkSafeUnref(((SkData*)ctx));
                          });
}


HBFont make_harfbuzz_font(const SkFont &font)
{
    int index = 0;
    std::unique_ptr<SkStreamAsset> typefaceAsset = font.getTypeface()->openExistingStream(&index);

    HBFace hbFace;

    if (typefaceAsset)
    {
        size_t size = typefaceAsset->getLength();

        HBBlob blob;
        if (const void *base = typefaceAsset->getMemoryBase())
        {
            blob.reset(hb_blob_create((char *)base, SkToUInt(size),
                                    HB_MEMORY_MODE_READONLY, typefaceAsset.release(),
                                    [](void *p)
                                    { delete (SkStreamAsset *)p; }));
        }
        else
        {
            void *ptr = size ? sk_malloc_throw(size) : nullptr;
            typefaceAsset->read(ptr, size);
            blob.reset(hb_blob_create((char *)ptr, SkToUInt(size),
                                    HB_MEMORY_MODE_READONLY, ptr, sk_free));
        }
        hb_blob_make_immutable(blob.get());

        hbFace.reset(hb_face_create(blob.get(), index));
        hb_face_set_index(hbFace.get(), index);
        hb_face_set_upem(hbFace.get(), font.getTypeface()->getUnitsPerEm());
    }
    

    if(!hbFace) 
    {
        hbFace.reset(hb_face_create_for_tables(
            skhb_get_table,
            const_cast<SkTypeface*>(SkRef(font.getTypeface())),
            [](void* user_data){ SkSafeUnref(reinterpret_cast<SkTypeface*>(user_data)); }));
        
        if(hbFace)
        {
            hb_face_set_index(hbFace.get(), (unsigned)index);
            hb_face_set_upem(hbFace.get(), font.getTypeface()->getUnitsPerEm());
        }
    }

    if(!hbFace) 
    {
        return HBFont();
    }

    HBFont hbFont(hb_font_create(hbFace.get()));
    float scale = font.getSize() * SkiaToHarfBuzzFontSize;
    hb_font_set_scale(hbFont.get(), scale, scale);
    hb_ot_font_set_funcs(hbFont.get());
    return hbFont;
}

void AlphaSkiaCanvas::text_run(const char16_t *text,
                               int text_length,
                               SkFont &font,
                               sk_sp<SkTextBlob> &realBlob,
                               float &width)
{
    font.setEdging(SkFont::Edging::kAntiAlias);
    font.setSubpixel(true);
    font.setHinting(SkFontHinting::kNormal);

    HBFont harfBuzzFont(make_harfbuzz_font(font));

    SkTextBlobBuilder builder;
    if(!harfBuzzFont)
    {
        auto runBuffer = builder.allocRunPos(font, 0);
        realBlob = builder.make();
        width = 0.0f;
        return;
    }

    HBBuffer buffer(hb_buffer_create());
    hb_buffer_set_direction(buffer.get(), HB_DIRECTION_LTR);
    hb_buffer_set_language(buffer.get(), hb_language_get_default());
    hb_buffer_add_utf16(buffer.get(), reinterpret_cast<const uint16_t*>(text), text_length, 0, -1);

    hb_shape(harfBuzzFont.get(), buffer.get(), nullptr, 0);

    uint32_t infosLength(0);
    hb_glyph_info_t *infos = hb_buffer_get_glyph_infos(buffer.get(),
                                                       &infosLength);
    uint32_t positionsLength(0);
    hb_glyph_position_t *positions = hb_buffer_get_glyph_positions(buffer.get(),
                                                                   &positionsLength);

    auto runBuffer = builder.allocRunPos(font, infosLength);

    auto glyphSpan = runBuffer.glyphs;
    auto positionSpan = runBuffer.points();

    width = 0.0f;
    for (uint32_t i = 0; i < infosLength; i++)
    {
        glyphSpan[i] = (SkGlyphID)infos[i].codepoint;

        auto xOffset = width + HarfBuzzToSkiaFontSize * positions[i].x_offset;
        auto yOffset = HarfBuzzToSkiaFontSize * -positions[i].y_offset;
        positionSpan[i] = {xOffset, yOffset};

        width += HarfBuzzToSkiaFontSize * positions[i].x_advance;
    }

    realBlob = builder.make();
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

const uint32_t AlphaSkiaCanvas::layoutUnitFractionalBits_ = 6;
const int AlphaSkiaCanvas::fixedPointDenominator_ = 1 << layoutUnitFractionalBits_;

int AlphaSkiaCanvas::float_to_layout_unit(float value)
{
    // https://source.chromium.org/chromium/chromium/src/+/main:third_party/blink/renderer/platform/geometry/layout_unit.h;l=147;drc=5a2e12875a8fe207bfe6f0febc782b6297788b6d;bpv=1;bpt=1?q=FromFloatRound&ss=chromium%2Fchromium%2Fsrc
    return static_cast<int>(roundf(value * fixedPointDenominator_));
}

float AlphaSkiaCanvas::layout_unit_to_float(int value)
{
    // https://source.chromium.org/chromium/chromium/src/+/main:third_party/blink/renderer/platform/geometry/layout_unit.h;l=147;drc=5a2e12875a8fe207bfe6f0febc782b6297788b6d;bpv=1;bpt=1?q=FromFloatRound&sq=&ss=chromium%2Fchromium%2Fsrc    return static_cast<int>(roundf(value * kFixedPointDenominator))
    return static_cast<float>(value) / fixedPointDenominator_;
}

bool AlphaSkiaCanvas::try_set_normalized_typo_ascent_and_descent(float em_height, float typo_ascent, float typo_descent, int &ascent, int &descent)
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

void AlphaSkiaCanvas::normalized_typo_ascent_and_descent(const SkFont &font, int &ascent, int &descent)
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

    int ascent(0);
    int descent(0);

    switch (baseline)
    {
    case alphaskia_text_baseline_alphabetic: // kAlphabeticTextBaseline
        return 0;
    case alphaskia_text_baseline_top: // kHangingTextBaseline
#define kHangingAsPercentOfAscent 80
        return float_ascent(metrics) * kHangingAsPercentOfAscent / 100.0f;
        break;
    case alphaskia_text_baseline_middle: // kMiddleTextBaseline
    {
        normalized_typo_ascent_and_descent(font, ascent, descent);
        auto middle = (layout_unit_to_float(ascent) - layout_unit_to_float(descent)) / 2.0f;
        return middle;
    }
    case alphaskia_text_baseline_bottom: // kBottomTextBaseline
        normalized_typo_ascent_and_descent(font, ascent, descent);
        return -layout_unit_to_float(descent);
        break;
    }

    return 0;
}

void AlphaSkiaCanvas::draw_image(sk_sp<SkImage> image, float x, float y, float w, float h)
{
    SkSamplingOptions sampling;
    surface_->getCanvas()->drawImageRect(image, SkRect::MakeXYWH(x, y, w, h), sampling);
}