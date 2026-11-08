#include "../include/TextRenderingTest.h"
#include "../include/AlphaSkiaTestBridge.h"

#include <climits>
#include <cmath>
#include <codecvt>
#include <fstream>
#include <iostream>
#include <locale>
#include <string>
#include <vector>

// The expectations follow the HTML5 canvas specification as implemented by Chromium
// (base_rendering_context_2d.cc DrawTextInternal and text_metrics.cc):
// - all glyphs, including those from later families or font fallback, share one alphabetic baseline,
// - the baseline offset for textBaseline and all font-derived metrics come from the first available font,
// - textAlign shifts by the full advance width,
// - ASCII whitespace is drawn and measured as U+0020.
// Checks compare renderings against each other instead of reference images so that they hold on every platform.

namespace
{
    const int canvas_width = 300;
    // fits the music font with baseline top, which places the glyph about its full ascent below y
    const int canvas_height = 240;

    // Tolerance between rasterized ink and measured glyph bounds, in logical pixels: one pixel of antialiasing
    // coverage plus up to half a pixel from Skia snapping the baseline of axis-aligned text to device pixels.
    const float ink_tolerance = 1.5f;

    // Maximum per-pixel alpha difference for renderings that must be identical up to floating point noise.
    const int alpha_tolerance = 16;

    // measure_text reports the width rounded to 1/100 px while drawing aligns with the unrounded width. Text placed at
    // a measured width may therefore land in a neighbouring horizontal subpixel step, in device pixels.
    const int width_rounding_tolerance = 1;

    struct TextStyle
    {
        std::string name;
        std::vector<const char *> families;
        uint16_t weight;
        bool italic;
        // glyphs missing in the families come from fonts installed on the system
        bool uses_system_fallback;
        alphaskia_text_style_t style;
    };

    struct TextCase
    {
        std::string name;
        // the text is drawn as prefix + primary_text + suffix
        std::u16string prefix;
        std::u16string suffix;
        bool has_emoji;

        std::u16string text() const;
    };

    const std::u16string primary_text = u"=E";

    std::u16string TextCase::text() const
    {
        return prefix + primary_text + suffix;
    }

    const std::vector<TextCase> text_cases = {
        {"later family, larger ascent (U+266D)", u"", u" \u266D", false},
        {"later family, larger ascent (U+1D11E)", u"", u" \U0001D11E", false},
        {"emoji, smaller ascent", u"", u" \U0001F3B8", true},
        {"fallback glyph first (U+266D)", u"\u266D ", u"", false},
        {"emoji first", u"\U0001F3B8 ", u"", true},
    };

    const std::vector<alphaskia_text_baseline_t> baselines = {
        alphaskia_text_baseline_alphabetic,
        alphaskia_text_baseline_top,
        alphaskia_text_baseline_middle,
        alphaskia_text_baseline_bottom};

    const std::vector<alphaskia_text_align_t> aligns = {
        alphaskia_text_align_left,
        alphaskia_text_align_center,
        alphaskia_text_align_right};

    const std::vector<float> render_scales = {1.0f, 2.0f};

    int checks = 0;
    int failures = 0;
    std::filesystem::path failure_output_path;
    int saved_failures = 0;

    std::string to_utf8(const std::u16string &text)
    {
        static std::wstring_convert<std::codecvt_utf8_utf16<char16_t>, char16_t> convert{};
        return convert.to_bytes(text);
    }

    const char *baseline_name(alphaskia_text_baseline_t baseline)
    {
        switch (baseline)
        {
        case alphaskia_text_baseline_alphabetic:
            return "alphabetic";
        case alphaskia_text_baseline_top:
            return "top";
        case alphaskia_text_baseline_middle:
            return "middle";
        case alphaskia_text_baseline_bottom:
            return "bottom";
        }
        return "?";
    }

    const char *align_name(alphaskia_text_align_t align)
    {
        switch (align)
        {
        case alphaskia_text_align_left:
            return "left";
        case alphaskia_text_align_center:
            return "center";
        case alphaskia_text_align_right:
            return "right";
        }
        return "?";
    }

    void check(bool condition, const std::string &message)
    {
        checks++;
        if (!condition)
        {
            failures++;
            std::cerr << "  FAILED: " << message << std::endl;
        }
    }

    // Inclusive bounds in device pixels.
    struct InkBounds
    {
        bool empty = true;
        int left = 0;
        int top = 0;
        int right = 0;
        int bottom = 0;

        std::string to_string() const
        {
            if (empty)
            {
                return "<empty>";
            }
            return "[l=" + std::to_string(left) + " t=" + std::to_string(top) +
                   " r=" + std::to_string(right) + " b=" + std::to_string(bottom) + "]";
        }

        bool operator==(const InkBounds &other) const
        {
            return empty == other.empty &&
                   (empty || (left == other.left && top == other.top && right == other.right && bottom == other.bottom));
        }
    };

    struct Raster
    {
        int width = 0;
        int height = 0;
        float scale = 1;
        std::vector<uint8_t> alpha;

        InkBounds ink(int from_x = 0, int to_x = INT_MAX) const
        {
            InkBounds bounds;
            for (int y = 0; y < height; y++)
            {
                for (int x = std::max(from_x, 0); x <= std::min(to_x, width - 1); x++)
                {
                    if (alpha[y * width + x] == 0)
                    {
                        continue;
                    }
                    if (bounds.empty)
                    {
                        bounds = {false, x, y, x, y};
                    }
                    else
                    {
                        bounds.left = std::min(bounds.left, x);
                        bounds.top = std::min(bounds.top, y);
                        bounds.right = std::max(bounds.right, x);
                        bounds.bottom = std::max(bounds.bottom, y);
                    }
                }
            }
            return bounds;
        }

        std::vector<bool> ink_rows() const
        {
            std::vector<bool> rows(height, false);
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width && !rows[y]; x++)
                {
                    rows[y] = alpha[y * width + x] != 0;
                }
            }
            return rows;
        }

        // Same rows with ink and horizontal ink extent within width_rounding_tolerance.
        bool matches_up_to_width_rounding(const Raster &other) const
        {
            auto a = ink();
            auto b = other.ink();
            return ink_rows() == other.ink_rows() && a.empty == b.empty &&
                   (a.empty || (std::abs(a.left - b.left) <= width_rounding_tolerance &&
                                std::abs(a.right - b.right) <= width_rounding_tolerance));
        }

        int max_alpha_difference(const Raster &other) const
        {
            int max = 0;
            for (size_t i = 0; i < alpha.size(); i++)
            {
                max = std::max(max, std::abs(static_cast<int>(alpha[i]) - static_cast<int>(other.alpha[i])));
            }
            return max;
        }
    };

    bool write_png(alphaskia_image_t image, const std::filesystem::path &file);

    // Keeps the renderings of the first failed comparisons for inspection.
    void save_failure(const std::string &name, const Raster &actual, const Raster *expected)
    {
        const int max_saved_failures = 10;
        if (failure_output_path.empty() || saved_failures >= max_saved_failures)
        {
            return;
        }
        saved_failures++;

        for (const auto &[suffix, raster] : {std::make_pair(".actual.png", &actual), std::make_pair(".expected.png", expected)})
        {
            if (raster == nullptr)
            {
                continue;
            }
            std::vector<uint8_t> pixels(raster->alpha.size() * 4);
            for (size_t i = 0; i < raster->alpha.size(); i++)
            {
                // black ink on white for viewing
                const uint8_t v = static_cast<uint8_t>(255 - raster->alpha[i]);
                pixels[i * 4] = v;
                pixels[i * 4 + 1] = v;
                pixels[i * 4 + 2] = v;
                pixels[i * 4 + 3] = 255;
            }
            auto image = alphaskia_image_from_pixels(raster->width, raster->height, pixels.data(), pixels.size());
            auto file = failure_output_path / ("text-rendering-failure-" + std::to_string(saved_failures) + "-" + name + suffix);
            if (write_png(image, file))
            {
                std::cerr << "  Saved " << file.generic_string() << std::endl;
            }
            alphaskia_image_free(image);
        }
    }

    template <typename Draw>
    Raster rasterize_on(alphaskia_canvas_t canvas, float scale, Draw draw)
    {
        alphaskia_canvas_begin_render(canvas, canvas_width, canvas_height, scale);
        alphaskia_canvas_set_color(canvas, alphaskia_rgba_to_color(0, 0, 0, 255));
        draw(canvas);
        auto image = alphaskia_canvas_end_render(canvas);

        Raster raster;
        raster.width = alphaskia_image_get_width(image);
        raster.height = alphaskia_image_get_height(image);
        raster.scale = scale;

        std::vector<uint8_t> pixels(static_cast<size_t>(raster.width) * raster.height * 4);
        alphaskia_image_read_pixels(image, pixels.data(), raster.width * 4, pixels.size());
        alphaskia_image_free(image);

        raster.alpha.resize(static_cast<size_t>(raster.width) * raster.height);
        for (size_t i = 0; i < raster.alpha.size(); i++)
        {
            // alpha is the 4th byte in both RGBA and BGRA
            raster.alpha[i] = pixels[i * 4 + 3];
        }
        return raster;
    }

    template <typename Draw>
    Raster rasterize(float scale, Draw draw)
    {
        auto canvas = alphaskia_canvas_new();
        auto raster = rasterize_on(canvas, scale, draw);
        alphaskia_canvas_free(canvas);
        return raster;
    }

    Raster rasterize_text(const std::u16string &text, const TextStyle &style, float font_size, float scale,
                          float x, float y, alphaskia_text_align_t align, alphaskia_text_baseline_t baseline)
    {
        return rasterize(scale, [&](alphaskia_canvas_t canvas)
                         { alphaskia_canvas_fill_text(canvas, text.c_str(), static_cast<int>(text.length()), style.style,
                                                      font_size, x, y, align, baseline); });
    }

    struct Metrics
    {
        float width;
        float actual_bounding_box_left;
        float actual_bounding_box_right;
        float font_bounding_box_ascent;
        float font_bounding_box_descent;
        float actual_bounding_box_ascent;
        float actual_bounding_box_descent;
        float em_height_ascent;
        float em_height_descent;
        float hanging_baseline;
        float alphabetic_baseline;
        float ideographic_baseline;
    };

    Metrics measure(const std::u16string &text, const TextStyle &style, float font_size,
                    alphaskia_text_align_t align, alphaskia_text_baseline_t baseline)
    {
        auto canvas = alphaskia_canvas_new();
        auto m = alphaskia_canvas_measure_text(canvas, text.c_str(), static_cast<int>(text.length()), style.style,
                                               font_size, align, baseline);
        Metrics metrics{
            alphaskia_text_metrics_get_width(m),
            alphaskia_text_metrics_get_actual_bounding_box_left(m),
            alphaskia_text_metrics_get_actual_bounding_box_right(m),
            alphaskia_text_metrics_get_font_bounding_box_ascent(m),
            alphaskia_text_metrics_get_font_bounding_box_descent(m),
            alphaskia_text_metrics_get_actual_bounding_box_ascent(m),
            alphaskia_text_metrics_get_actual_bounding_box_descent(m),
            alphaskia_text_metrics_get_em_height_ascent(m),
            alphaskia_text_metrics_get_em_height_descent(m),
            alphaskia_text_metrics_get_hanging_baseline(m),
            alphaskia_text_metrics_get_alphabetic_baseline(m),
            alphaskia_text_metrics_get_ideographic_baseline(m)};
        alphaskia_text_metrics_free(m);
        alphaskia_canvas_free(canvas);
        return metrics;
    }

    std::string describe(const TextStyle &style, const std::u16string &text, float font_size, float scale,
                         alphaskia_text_align_t align, alphaskia_text_baseline_t baseline)
    {
        return "style=" + style.name + " text='" + to_utf8(text) + "' size=" + std::to_string(font_size) +
               " scale=" + std::to_string(scale) + " align=" + align_name(align) + " baseline=" + baseline_name(baseline);
    }

    void check_near(float actual, float expected, float tolerance, const std::string &what, const std::string &context)
    {
        check(std::abs(actual - expected) <= tolerance,
              what + " is " + std::to_string(actual) + ", expected " + std::to_string(expected) + " (" + context + ")");
    }

    // All glyphs share the baseline of the primary font: drawing the text at once equals drawing its parts one after another.
    // In particular the glyphs of the primary font stay where they are without the glyphs of other fonts.
    void test_mixed_text_equals_its_parts(const TextStyle &style)
    {
        const float x = 10;
        const float y = 40;
        for (float font_size : {12.0f, 24.0f})
        {
            for (float scale : render_scales)
            {
                for (auto baseline : baselines)
                {
                    for (const auto &text_case : text_cases)
                    {
                        const auto text = text_case.text();
                        auto context = "[" + text_case.name + "] (" +
                                       describe(style, text, font_size, scale, alphaskia_text_align_left, baseline) + ")";

                        auto whole = rasterize_text(text, style, font_size, scale, x, y, alphaskia_text_align_left, baseline);
                        auto parts = rasterize(scale, [&](alphaskia_canvas_t canvas)
                                               {
                            float part_x = x;
                            for (const auto &part : {text_case.prefix, primary_text, text_case.suffix})
                            {
                                if (part.empty())
                                {
                                    continue;
                                }
                                alphaskia_canvas_fill_text(canvas, part.c_str(), static_cast<int>(part.length()), style.style,
                                                           font_size, part_x, y, alphaskia_text_align_left, baseline);
                                part_x += measure(part, style, font_size, alphaskia_text_align_left, baseline).width;
                            } });

                        const bool equal = whole.matches_up_to_width_rounding(parts);
                        check(equal, "text drawn at once " + whole.ink().to_string() + " differs from its parts drawn separately " +
                                         parts.ink().to_string() + " " + context);
                        if (!equal)
                        {
                            save_failure("mixed-text", whole, &parts);
                        }
                    }
                }
            }
        }
    }

    // Font-derived metrics come from the first available font, glyph bounds match the drawn ink.
    void test_measure_matches_drawing(const TextStyle &style, const std::u16string &text,
                                      const std::u16string &primary_only_text, bool has_emoji, float font_size)
    {
        const float x = 150;
        const float y = 120;
        for (float scale : render_scales)
        {
            for (auto baseline : baselines)
            {
                for (auto align : aligns)
                {
                    auto context = describe(style, text, font_size, scale, align, baseline);
                    auto m = measure(text, style, font_size, align, baseline);
                    auto p = measure(primary_only_text, style, font_size, align, baseline);

                    const float metric_tolerance = 0.001f;
                    check_near(m.font_bounding_box_ascent, p.font_bounding_box_ascent, metric_tolerance, "fontBoundingBoxAscent", context);
                    check_near(m.font_bounding_box_descent, p.font_bounding_box_descent, metric_tolerance, "fontBoundingBoxDescent", context);
                    check_near(m.em_height_ascent, p.em_height_ascent, metric_tolerance, "emHeightAscent", context);
                    check_near(m.em_height_descent, p.em_height_descent, metric_tolerance, "emHeightDescent", context);
                    check_near(m.hanging_baseline, p.hanging_baseline, metric_tolerance, "hangingBaseline", context);
                    check_near(m.alphabetic_baseline, p.alphabetic_baseline, metric_tolerance, "alphabeticBaseline", context);
                    check_near(m.ideographic_baseline, p.ideographic_baseline, metric_tolerance, "ideographicBaseline", context);

                    auto raster = rasterize_text(text, style, font_size, scale, x, y, align, baseline);
                    auto ink = raster.ink();
                    check(!ink.empty, "nothing drawn (" + context + ")");
                    if (ink.empty)
                    {
                        continue;
                    }

                    const float ink_left = ink.left / scale;
                    const float ink_top = ink.top / scale;
                    const float ink_right = (ink.right + 1) / scale;
                    const float ink_bottom = (ink.bottom + 1) / scale;

                    const float box_left = x - m.actual_bounding_box_left;
                    const float box_top = y - m.actual_bounding_box_ascent;
                    const float box_right = x + m.actual_bounding_box_right;
                    const float box_bottom = y + m.actual_bounding_box_descent;

                    const std::string boxes = " ink=[" + std::to_string(ink_left) + ", " + std::to_string(ink_top) + ", " +
                                              std::to_string(ink_right) + ", " + std::to_string(ink_bottom) + "] box=[" +
                                              std::to_string(box_left) + ", " + std::to_string(box_top) + ", " +
                                              std::to_string(box_right) + ", " + std::to_string(box_bottom) + "] (" + context + ")";

                    const bool ink_inside = ink_left >= box_left - ink_tolerance && ink_top >= box_top - ink_tolerance &&
                                            ink_right <= box_right + ink_tolerance && ink_bottom <= box_bottom + ink_tolerance;
                    check(ink_inside, "ink outside of measured actualBoundingBox" + boxes);

                    // Color emoji bitmaps contain transparent margins, so their glyph bounds are larger than the visible ink.
                    // Glyph bounds of system fonts are not under control of the test.
                    const bool box_tight = has_emoji || style.uses_system_fallback ||
                                           (ink_left <= box_left + ink_tolerance && ink_top <= box_top + ink_tolerance &&
                                            ink_right >= box_right - ink_tolerance && ink_bottom >= box_bottom - ink_tolerance);
                    check(box_tight, "measured actualBoundingBox larger than ink" + boxes);

                    if (!ink_inside || !box_tight)
                    {
                        save_failure("measure", raster, nullptr);
                    }
                }
            }
        }
    }

    // textAlign center/right shift the text by half/full of the measured advance width.
    void test_horizontal_alignment(const TextStyle &style, const std::u16string &text, float font_size)
    {
        const float x = 150;
        const float y = 40;
        for (float scale : render_scales)
        {
            auto width = measure(text, style, font_size, alphaskia_text_align_left, alphaskia_text_baseline_alphabetic).width;
            for (auto align : {alphaskia_text_align_center, alphaskia_text_align_right})
            {
                const float shift = align == alphaskia_text_align_center ? width / 2 : width;
                auto aligned = rasterize_text(text, style, font_size, scale, x, y, align, alphaskia_text_baseline_alphabetic);
                auto shifted = rasterize_text(text, style, font_size, scale, x - shift, y, alphaskia_text_align_left,
                                              alphaskia_text_baseline_alphabetic);
                check(aligned.matches_up_to_width_rounding(shifted),
                      "aligned text " + aligned.ink().to_string() + " differs from left aligned text at x - " +
                          std::to_string(shift) + " " + shifted.ink().to_string() + " (" +
                          describe(style, text, font_size, scale, align, alphaskia_text_baseline_alphabetic) + ")");
            }
        }
    }

    // The advance width includes leading and trailing whitespace.
    void test_width_includes_whitespace(const TextStyle &style, float font_size)
    {
        auto width = [&](const std::u16string &text)
        { return measure(text, style, font_size, alphaskia_text_align_left, alphaskia_text_baseline_alphabetic).width; };

        const float plain = width(primary_text);
        const float space = width(u" ");
        check(space > 0, "width of U+0020 is " + std::to_string(space) + " (style=" + style.name + ")");
        check_near(width(u" " + primary_text), plain + space, 0.01f, "width with leading space", "style=" + style.name);
        check_near(width(primary_text + u" "), plain + space, 0.01f, "width with trailing space", "style=" + style.name);
    }

    // ASCII whitespace (TAB, LF, FF, CR) and VT, like in Chromium, are replaced by U+0020 before measuring and drawing.
    // U+2028 LINE SEPARATOR is checked as well because it would otherwise start a new line.
    void test_whitespace_is_space(const TextStyle &style, float font_size)
    {
        const std::u16string spaced = u"=E \u266D";
        for (char16_t whitespace : {u'\t', u'\n', u'\v', u'\f', u'\r', u'\u2028'})
        {
            std::u16string text = u"=E";
            text += whitespace;
            text += u"\u266D";

            const std::string name = "whitespace U+" + std::to_string(static_cast<int>(whitespace));
            auto expected_metrics = measure(spaced, style, font_size, alphaskia_text_align_left, alphaskia_text_baseline_alphabetic);
            auto actual_metrics = measure(text, style, font_size, alphaskia_text_align_left, alphaskia_text_baseline_alphabetic);
            check_near(actual_metrics.width, expected_metrics.width, 0.001f, "width", name);

            auto expected = rasterize_text(spaced, style, font_size, 1, 10, 40, alphaskia_text_align_left, alphaskia_text_baseline_alphabetic);
            auto actual = rasterize_text(text, style, font_size, 1, 10, 40, alphaskia_text_align_left, alphaskia_text_baseline_alphabetic);
            auto difference = actual.max_alpha_difference(expected);
            check(actual.ink() == expected.ink() && difference <= alpha_tolerance,
                  name + " drawn as " + actual.ink().to_string() + ", expected like U+0020 " + expected.ink().to_string());
        }
    }

    // Families which are not available are skipped when determining the first available font.
    void test_unavailable_family_is_skipped(const TextStyle &style, const TextStyle &with_missing_family, float font_size)
    {
        for (auto baseline : baselines)
        {
            std::vector<std::u16string> texts = {primary_text};
            for (const auto &text_case : text_cases)
            {
                texts.push_back(text_case.text());
            }

            for (const auto &text : texts)
            {
                auto context = describe(with_missing_family, text, font_size, 1, alphaskia_text_align_left, baseline);
                auto expected = measure(text, style, font_size, alphaskia_text_align_left, baseline);
                auto actual = measure(text, with_missing_family, font_size, alphaskia_text_align_left, baseline);
                check_near(actual.font_bounding_box_ascent, expected.font_bounding_box_ascent, 0.001f, "fontBoundingBoxAscent", context);
                check_near(actual.alphabetic_baseline, expected.alphabetic_baseline, 0.001f, "alphabeticBaseline", context);

                auto expected_raster = rasterize_text(text, style, font_size, 1, 10, 40, alphaskia_text_align_left, baseline);
                auto actual_raster = rasterize_text(text, with_missing_family, font_size, 1, 10, 40, alphaskia_text_align_left, baseline);
                check(actual_raster.max_alpha_difference(expected_raster) == 0, "rendering differs from style without unavailable family (" + context + ")");
            }
        }
    }

    // Fonts installed on the operating system are available in both font modes.
    void test_system_font_is_available(const TextStyle &system_style)
    {
        auto typeface = alphaskia_typeface_make_from_name(system_font_family, 400, 0);
        check(typeface != nullptr, std::string("typeface for ") + system_font_family);
        auto family_name = alphaskia_typeface_get_family_name(typeface);
        check(family_name && alphaskia_string_get_length(family_name) > 0, std::string("family name of the typeface for ") + system_font_family);
        alphaskia_string_free(family_name);
        alphaskia_typeface_free(typeface);

        auto raster = rasterize_text(u"=E", system_style, 24, 1, 10, 40, alphaskia_text_align_left, alphaskia_text_baseline_alphabetic);
        check(!raster.ink().empty, "text in a system font is drawn");
    }

    // Registered fonts are found by their family name in both font modes, the operating system does not know them.
    void test_registered_font_is_found_by_name()
    {
        struct Expected
        {
            const char *family;
            uint16_t weight;
            uint8_t italic;
        };
        for (const auto &expected : {Expected{"Bravura", 400, 0}, Expected{"Noto Serif", 700, 1}})
        {
            const std::string context = std::string(expected.family) + " " + std::to_string(expected.weight) +
                                        (expected.italic ? " italic" : "");
            auto typeface = alphaskia_typeface_make_from_name(expected.family, expected.weight, expected.italic);
            auto family_name = alphaskia_typeface_get_family_name(typeface);
            check(family_name && std::string(alphaskia_string_get_utf8(family_name)) == expected.family,
                  "family of the typeface by name (" + context + ")");
            check(alphaskia_typeface_get_weight(typeface) == expected.weight &&
                      alphaskia_typeface_is_italic(typeface) == expected.italic,
                  "style of the typeface by name (" + context + ")");
            alphaskia_string_free(family_name);
            alphaskia_typeface_free(typeface);
        }
    }

    // Purging the font caches releases fonts which are still in use, they are loaded again with the same rendering.
    void test_purge_font_caches(const std::vector<const TextStyle *> &styles)
    {
        const std::u16string text = u"=E ♭ \U0001F3B8";
        auto typeface = alphaskia_typeface_make_from_name(system_font_family, 700, 1);
        const uint16_t weight = alphaskia_typeface_get_weight(typeface);
        const uint8_t italic = alphaskia_typeface_is_italic(typeface);

        auto canvas = alphaskia_canvas_new();
        for (const auto *style : styles)
        {
            auto draw = [&](alphaskia_canvas_t target)
            {
                alphaskia_canvas_fill_text(target, text.c_str(), static_cast<int>(text.length()), style->style, 24, 10, 40,
                                           alphaskia_text_align_left, alphaskia_text_baseline_alphabetic);
            };

            auto before = rasterize_on(canvas, 1, draw);
            alphaskia_purge_font_caches();
            auto same_canvas = rasterize_on(canvas, 1, draw);
            alphaskia_purge_font_caches();
            auto new_canvas = rasterize(1, draw);

            check(!before.ink().empty, "text is drawn (style=" + style->name + ")");
            check(same_canvas.max_alpha_difference(before) == 0, "rendering on the same canvas changed after purging the font caches (style=" + style->name + ")");
            check(new_canvas.max_alpha_difference(before) == 0, "rendering on a new canvas changed after purging the font caches (style=" + style->name + ")");
        }
        alphaskia_canvas_free(canvas);

        auto family_name = alphaskia_typeface_get_family_name(typeface);
        check(family_name && alphaskia_string_get_length(family_name) > 0, "family name of a typeface after purging the font caches");
        check(alphaskia_typeface_get_weight(typeface) == weight && alphaskia_typeface_is_italic(typeface) == italic,
              "style of a typeface after purging the font caches");
        alphaskia_string_free(family_name);
        alphaskia_typeface_free(typeface);
    }

    bool write_png(alphaskia_image_t image, const std::filesystem::path &file)
    {
        auto png = alphaskia_image_encode_png(image);
        if (!png)
        {
            return false;
        }
        std::ofstream stream(file, std::ios::binary);
        stream.write(reinterpret_cast<const char *>(alphaskia_data_get_data(png)),
                     static_cast<std::streamsize>(alphaskia_data_get_length(png)));
        alphaskia_data_free(png);
        return static_cast<bool>(stream);
    }

    // Overview for visual review: every case per baseline, with the target y as red line and the target x as blue line.
    void write_overview(const TextStyle &style, const std::filesystem::path &file)
    {
        const float font_size = 24;
        const float column_width = 180;
        const float row_height = 70;

        std::vector<std::u16string> texts = {primary_text};
        for (const auto &text_case : text_cases)
        {
            texts.push_back(text_case.text());
        }

        const int width = static_cast<int>(column_width * aligns.size() * texts.size());
        const int height = static_cast<int>(row_height * baselines.size());

        auto canvas = alphaskia_canvas_new();
        alphaskia_canvas_begin_render(canvas, width, height, 1);
        alphaskia_canvas_set_color(canvas, alphaskia_rgba_to_color(255, 255, 255, 255));
        alphaskia_canvas_fill_rect(canvas, 0, 0, static_cast<float>(width), static_cast<float>(height));

        for (size_t row = 0; row < baselines.size(); row++)
        {
            const float y = row * row_height + row_height / 2;
            alphaskia_canvas_set_color(canvas, alphaskia_rgba_to_color(255, 0, 0, 255));
            alphaskia_canvas_fill_rect(canvas, 0, y, static_cast<float>(width), 1);

            size_t column = 0;
            for (const auto &text : texts)
            {
                for (auto align : aligns)
                {
                    const float cell_x = column * column_width;
                    const float x = align == alphaskia_text_align_left     ? cell_x + 20
                                    : align == alphaskia_text_align_center ? cell_x + column_width / 2
                                                                           : cell_x + column_width - 20;
                    alphaskia_canvas_set_color(canvas, alphaskia_rgba_to_color(0, 0, 255, 255));
                    alphaskia_canvas_fill_rect(canvas, x, y - row_height / 2 + 5, 1, row_height - 10);

                    alphaskia_canvas_set_color(canvas, alphaskia_rgba_to_color(0, 0, 0, 255));
                    alphaskia_canvas_fill_text(canvas, text.c_str(), static_cast<int>(text.length()), style.style,
                                               font_size, x, y, align, baselines[row]);
                    column++;
                }
            }
        }

        auto image = alphaskia_canvas_end_render(canvas);
        alphaskia_canvas_free(canvas);
        if (write_png(image, file))
        {
            std::cout << "  Overview saved to " << file.generic_string() << std::endl;
        }
        else
        {
            std::cerr << "  Could not save overview to " << file.generic_string() << std::endl;
        }
        alphaskia_image_free(image);
    }

    TextStyle make_style(const std::string &name, std::vector<const char *> families, uint16_t weight, bool italic, bool uses_system_fallback = false)
    {
        TextStyle style{name, families, weight, italic, uses_system_fallback, nullptr};
        style.style = alphaskia_text_style_new(static_cast<uint8_t>(style.families.size()),
                                               const_cast<const char **>(style.families.data()), weight, italic ? 1 : 0);
        return style;
    }
}

bool text_rendering_test(const std::filesystem::path &overview_output_file)
{
    std::cout << "BEGIN: text_rendering_test" << std::endl;
    checks = 0;
    failures = 0;
    saved_failures = 0;
    failure_output_path = overview_output_file.parent_path();

    std::vector<TextStyle> styles = {
        make_style("serif", {"Noto Serif", "Noto Music", "Noto Color Emoji"}, 400, false),
        make_style("serif-italic", {"Noto Serif", "Noto Music", "Noto Color Emoji"}, 400, true),
        make_style("sans", {"Noto Sans", "Noto Music", "Noto Color Emoji"}, 400, false),
        make_style("serif-only", {"Noto Serif"}, 400, false, true),
    };
    auto missing_family = make_style("missing-family", {"Unavailable Family", "Noto Serif", "Noto Music", "Noto Color Emoji"}, 400, false);
    auto system = make_style("system", {system_font_family}, 400, false, true);
    auto music = make_style("music", {"Bravura"}, 400, false);

    for (const auto &style : styles)
    {
        std::cout << "  mixed text equals its parts: " << style.name << std::endl;
        test_mixed_text_equals_its_parts(style);

        std::cout << "  measure matches drawing: " << style.name << std::endl;
        test_measure_matches_drawing(style, primary_text, primary_text, false, 24);
        for (const auto &text_case : text_cases)
        {
            test_measure_matches_drawing(style, text_case.text(), primary_text, text_case.has_emoji, 24);
        }

        std::cout << "  width includes whitespace: " << style.name << std::endl;
        test_width_includes_whitespace(style, 24);

        std::cout << "  horizontal alignment: " << style.name << std::endl;
        for (const auto &text : {primary_text, std::u16string(u"=E "), std::u16string(u"  =E"), std::u16string(u"=E \u266D "),
                                 std::u16string(u"\u266D =E"), std::u16string(u"\U0001F3B8 =E")})
        {
            test_horizontal_alignment(style, text, 24);
        }
    }

    std::cout << "  music font symbols" << std::endl;
    // noteheadBlack, gClef, accidentalFlat
    for (const auto &symbols : {std::u16string(u"\uE0A4"), std::u16string(u"\uE050"), std::u16string(u"\uE260\uE0A4")})
    {
        test_measure_matches_drawing(music, symbols, symbols, false, 34);
        test_horizontal_alignment(music, symbols, 34);
    }

    std::cout << "  whitespace" << std::endl;
    test_whitespace_is_space(styles[0], 24);

    std::cout << "  unavailable family" << std::endl;
    test_unavailable_family_is_skipped(styles[0], missing_family, 24);

    std::cout << "  system fonts" << std::endl;
    test_system_font_is_available(system);

    std::cout << "  registered fonts by name" << std::endl;
    test_registered_font_is_found_by_name();

    std::cout << "  purge font caches" << std::endl;
    test_purge_font_caches({&styles[0], &system});

    write_overview(styles[1], overview_output_file);

    for (auto &style : styles)
    {
        alphaskia_text_style_free(style.style);
    }
    alphaskia_text_style_free(missing_family.style);
    alphaskia_text_style_free(system.style);
    alphaskia_text_style_free(music.style);

    std::cout << "  " << (checks - failures) << "/" << checks << " checks passed" << std::endl;
    if (failures > 0)
    {
        std::cout << "FAILED: text_rendering_test" << std::endl;
        return false;
    }

    std::cout << "SUCCESS: text_rendering_test" << std::endl;
    return true;
}
