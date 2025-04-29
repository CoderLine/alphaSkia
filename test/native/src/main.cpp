#include "../include/AlphaTabGeneratedTest.h"
#include "../include/PixelMatch.h"

#include <filesystem>
#include <iostream>
#include <fstream>

#include <stdio.h>
#include <string.h>
#include <codecvt>

#ifndef ALPHASKIA_TEST_RID
#error "Missing alphaSkia runtime identifier, please specify ALPHASKIA_TEST_RID";
#endif

#define STRINGIFY(s) _STRINGIFY(s)
#define _STRINGIFY(s) #s

const double tolerance_percent = 1;

std::filesystem::path find_repository_root(std::filesystem::path current, bool &success)
{
    if (std::filesystem::exists(current / ".nuke"))
    {
        std::cout << "Repository root found at " << current << std::endl;
        success = true;
        return current;
    }

    if (!current.has_parent_path())
    {
        std::cerr << "Could not find repository root" << std::endl;
        success = false;
        return std::filesystem::path();
    }

    return find_repository_root(current.parent_path(), success);
}

alphaskia_image_t render_full_image()
{
    alphaskia_canvas_t full_image_canvas = alphaskia_canvas_new();
    std::cout << "Begin render with size " << total_width << "x" << total_height << " at scale" << render_scale << std::endl;
    alphaskia_canvas_begin_render(full_image_canvas, total_width, total_height, render_scale);

    alphaskia_canvas_t partial_canvas = alphaskia_canvas_new();
    for (uint32_t i = 0; i < all_parts.size(); i++)
    {
        std::cout << "Render part " << i << std::endl;
        alphaskia_image_t part = all_parts[i](partial_canvas);

        float x = part_positions[i][0];
        float y = part_positions[i][1];
        float w = part_positions[i][2];
        float h = part_positions[i][3];

        std::cout << "Drawing part " << i << "into full image at " << x << "/" << y << "/" << w << "/" << h << std::endl;
        alphaskia_canvas_draw_image(full_image_canvas, part, x, y, w, h);
        alphaskia_image_free(part);
    }
    alphaskia_canvas_free(partial_canvas);

    alphaskia_image_t full_image = alphaskia_canvas_end_render(full_image_canvas);
    alphaskia_canvas_free(full_image_canvas);
    std::cout << "Render of full image completed" << std::endl;

    return full_image;
}

bool write_data_to_file_and_free(alphaskia_data_t data, std::string path)
{
    std::ofstream stream(path, std::ios::binary);
    if (!stream)
    {
        alphaskia_data_free(data);
        std::cerr << "Could not open file " << path << std::endl;
        return false;
    }

    uint8_t *data_raw = alphaskia_data_get_data(data);
    uint64_t data_length = alphaskia_data_get_length(data);
    stream.write(reinterpret_cast<char *>(data_raw), static_cast<std::streamsize>(data_length));
    stream.flush();
    stream.close();

    alphaskia_data_free(data);
    return true;
}

bool empty_image_test()
{
    // https://github.com/CoderLine/alphaSkia/issues/53
    std::cout << "BEGIN: empty_image_test" << std::endl;
    auto canvas = alphaskia_canvas_new();
    alphaskia_canvas_begin_render(canvas, 0, 0, 1);

    auto empty = alphaskia_canvas_end_render(canvas);

    alphaskia_image_free(empty);

    alphaskia_canvas_free(canvas);
    std::cout << "SUCCESS: empty_image_test" << std::endl;
    return true;
}

bool reference_image_test(const std::filesystem::path &repository_root,
                          const std::filesystem::path &test_data_path,
                          bool isFreeType,
                          alphaskia_image_t actual_image,
                          const std::string &test_file_name)
{
    // save png for future reference
    std::cout << "Saving image as PNG" << std::endl;

    std::filesystem::path test_output_path = repository_root / "test" / "test-outputs" / "native";
    std::filesystem::create_directories(test_output_path);

    std::string test_output_file_base = test_file_name + (isFreeType ? "freetype" : STRINGIFY(ALPHASKIA_TEST_RID));

    std::filesystem::path test_output_file = test_output_path / (test_output_file_base + ".png");
    alphaskia_data_t png_data = alphaskia_image_encode_png(actual_image);
    if (!png_data)
    {
        std::cerr << "Failed to encode final image to png" << std::endl;
        return false;
    }
    write_data_to_file_and_free(png_data, test_output_file.generic_string());
    std::cout << "Image saved to " << test_output_file.generic_string() << std::endl;

    // load reference image
    std::filesystem::path test_reference_path = test_data_path / "reference" / (test_output_file_base + ".png");
    std::cout << "Loading reference image " << test_reference_path.generic_string() << std::endl;
    std::vector<uint8_t> test_reference_data;
    read_file(test_reference_path.generic_string(), test_reference_data);
    alphaskia_image_t expected_image = alphaskia_image_decode(test_reference_data.data(), test_reference_data.size());
    if (!expected_image)
    {
        std::cerr << "Failed to decode reference image" << std::endl;
        return false;
    }
    std::cout << "Reference image loaded" << std::endl;

    // compare images
    std::cout << "Comparing images" << std::endl;
    auto actual_width = alphaskia_image_get_width(actual_image);
    auto actual_height = alphaskia_image_get_height(actual_image);

    auto expected_width = alphaskia_image_get_width(expected_image);
    auto expected_height = alphaskia_image_get_height(expected_image);
    if (actual_width != expected_width || actual_height != expected_height)
    {
        std::cerr << "Image sizes do not match: Actual[" << actual_width << "x" << actual_height << "] Expected[" << expected_width << "x" << actual_height << "]" << std::endl;
        return false;
    }

    std::vector<uint8_t> actual_pixels;
    std::vector<uint8_t> expected_pixels;
    std::vector<uint8_t> diff_pixels;

    size_t row_bytes = actual_width * sizeof(uint32_t);
    size_t pixel_bytes = row_bytes * actual_height;
    actual_pixels.resize(pixel_bytes);
    expected_pixels.resize(pixel_bytes);
    diff_pixels.resize(pixel_bytes);

    alphaskia_image_read_pixels(actual_image, actual_pixels.data(), row_bytes);
    alphaskia_image_read_pixels(expected_image, expected_pixels.data(), row_bytes);

    auto compare_result = pixel_match(actual_pixels,
                                      expected_pixels,
                                      diff_pixels,
                                      actual_width,
                                      actual_height,
                                      {/* threshold: */ 0.3,
                                       /* diff_color: */ {255, 0, 0}});

    int32_t totalPixels = compare_result.total_pixels - compare_result.transparent_pixels;
    double percentDifference = ((double)compare_result.different_pixels / totalPixels) * 100;
    bool pass = percentDifference < tolerance_percent;
    if (!pass)
    {
        std::cerr << "Difference between original and new image is too big: " << compare_result.different_pixels << "/" << totalPixels << "(" << percentDifference << "%)";

        // create diff image as PNG
        alphaskia_image_t diff_image = alphaskia_image_from_pixels(actual_width, actual_height, diff_pixels.data());
        alphaskia_data_t diff_image_png_data = alphaskia_image_encode_png(diff_image);
        alphaskia_image_free(diff_image);

        auto diff_output_path = test_output_path / (test_output_file_base + ".diff.png");
        write_data_to_file_and_free(diff_image_png_data, diff_output_path.generic_string());
        std::cout << "Error diff image saved to " << diff_output_path.generic_string() << std::endl;

        // for the sake of comparing directly, we also store the old image (we had cases where linux detected differences on the exact same file)
        alphaskia_image_t old_image = alphaskia_image_from_pixels(actual_width, actual_height, expected_pixels.data());
        alphaskia_data_t old_image_png_data = alphaskia_image_encode_png(old_image);
        alphaskia_image_free(old_image);

        auto old_output_path = test_output_path / (test_output_file_base + ".old.png");
        write_data_to_file_and_free(old_image_png_data, old_output_path.generic_string());
        std::cout << "Error old image saved to " << old_output_path.generic_string() << std::endl;

        return false;
    }

    std::cout << "Images match. Total Pixels: " << compare_result.total_pixels << ", Transparent Pixels: " << compare_result.transparent_pixels << ", Percent difference:" << percentDifference << "%" << std::endl;
    return true;
}

bool music_sheet_test(const std::filesystem::path &repository_root, const std::filesystem::path &test_data_path, bool isFreeType)
{
    std::cout << "BEGIN: music_sheet_test" << std::endl;

    // render full image
    std::cout << "Rendering image" << std::endl;
    alphaskia_image_t actual_image = render_full_image();
    std::cout << "Image rendered" << std::endl;

    if (!reference_image_test(repository_root, test_data_path, isFreeType, actual_image, ""))
    {
        std::cout << "FAILED: music_sheet_test" << std::endl;
        return false;
    }

    std::cout << "SUCCESS: music_sheet_test" << std::endl;
    return true;
}

void measure_text_draw(alphaskia_canvas_t canvas,
                       alphaskia_text_style_t text_style,
                       const std::vector<std::u16string> &baselines,
                       size_t &index)
{
    static std::wstring_convert<std::codecvt_utf8_utf16<char16_t>, char16_t> convert{};

    for (auto &baseline : baselines)
    {
        std::u16string text = std::u16string(u"Abcdefghijklmnop (") + baseline + u")";

        auto text_metrics = alphaskia_canvas_measure_text(
            canvas,
            text.c_str(),
            static_cast<int>(text.length()),
            text_style,
            18,
            alphaskia_text_align_left,
            alphaskia_text_baseline_alphabetic);

        std::cout << convert.to_bytes(text.data()) << std::endl;
        std::cout << "  get_actual_bounding_box_ascent: " << alphaskia_text_metrics_get_actual_bounding_box_ascent(text_metrics) << std::endl;
        std::cout << "  get_actual_bounding_box_descent: " << alphaskia_text_metrics_get_actual_bounding_box_descent(text_metrics) << std::endl;
        std::cout << "  get_actual_bounding_box_left: " << alphaskia_text_metrics_get_actual_bounding_box_left(text_metrics) << std::endl;
        std::cout << "  get_actual_bounding_box_right: " << alphaskia_text_metrics_get_actual_bounding_box_right(text_metrics) << std::endl;
        std::cout << "  get_alphabetic_baseline: " << alphaskia_text_metrics_get_alphabetic_baseline(text_metrics) << std::endl;
        std::cout << "  get_font_bounding_box_ascent: " << alphaskia_text_metrics_get_font_bounding_box_ascent(text_metrics) << std::endl;
        std::cout << "  get_font_bounding_box_descent: " << alphaskia_text_metrics_get_font_bounding_box_descent(text_metrics) << std::endl;
        std::cout << "  get_ideographic_baseline: " << alphaskia_text_metrics_get_ideographic_baseline(text_metrics) << std::endl;
        std::cout << "  get_hanging_baseline: " << alphaskia_text_metrics_get_hanging_baseline(text_metrics) << std::endl;
        std::cout << "  get_width: " << alphaskia_text_metrics_get_width(text_metrics) << std::endl;
        std::cout << "  get_em_height_ascent: " << alphaskia_text_metrics_get_em_height_ascent(text_metrics) << std::endl;
        std::cout << "  get_em_height_descent: " << alphaskia_text_metrics_get_em_height_descent(text_metrics) << std::endl;

        const int y = 50 + (index++) * 50;
        alphaskia_canvas_set_color(canvas, alphaskia_rgba_to_color(0, 0, 0, 255));
        alphaskia_canvas_fill_text(
            canvas,
            text.c_str(),
            static_cast<int>(text.length()),
            text_style,
            18,
            0, y,
            alphaskia_text_align_left,
            alphaskia_text_baseline_alphabetic);

        float baselineMetricValue = 0;
        bool aboveAlphabetic = false;
        if (baseline == u"fontBoundingBoxAscent")
        {
            baselineMetricValue = alphaskia_text_metrics_get_font_bounding_box_ascent(text_metrics) * -1;
            aboveAlphabetic = true;
        }
        else if (baseline == u"actualBoundingBoxAscent")
        {
            baselineMetricValue = alphaskia_text_metrics_get_actual_bounding_box_ascent(text_metrics) * -1;
            aboveAlphabetic = true;
        }
        else if (baseline == u"emHeightAscent")
        {
            baselineMetricValue = alphaskia_text_metrics_get_em_height_ascent(text_metrics) * -1;
            aboveAlphabetic = true;
        }
        else if (baseline == u"hangingBaseline")
        {
            baselineMetricValue = alphaskia_text_metrics_get_hanging_baseline(text_metrics) * -1;
            aboveAlphabetic = true;
        }
        else if (baseline == u"ideographicBaseline")
        {
            baselineMetricValue = std::abs(alphaskia_text_metrics_get_ideographic_baseline(text_metrics));
        }
        else if (baseline == u"emHeightDescent")
        {
            baselineMetricValue = alphaskia_text_metrics_get_em_height_descent(text_metrics);
        }
        else if (baseline == u"actualBoundingBoxDescent")
        {
            baselineMetricValue = alphaskia_text_metrics_get_actual_bounding_box_descent(text_metrics);
        }
        else if (baseline == u"fontBoundingBoxDescent")
        {
            baselineMetricValue = alphaskia_text_metrics_get_font_bounding_box_descent(text_metrics);
        }
        else if (baseline == u"alphabeticBaseline")
        {
            baselineMetricValue = alphaskia_text_metrics_get_alphabetic_baseline(text_metrics);
        }

        const float lineY = y + baselineMetricValue;
        alphaskia_canvas_set_color(canvas, alphaskia_rgba_to_color(255, 0, 0, 255));
        alphaskia_canvas_begin_path(canvas);
        alphaskia_canvas_move_to(canvas, alphaskia_text_metrics_get_actual_bounding_box_left(text_metrics), y);
        alphaskia_canvas_line_to(canvas, alphaskia_text_metrics_get_actual_bounding_box_left(text_metrics), lineY);
        alphaskia_canvas_line_to(canvas, alphaskia_text_metrics_get_actual_bounding_box_right(text_metrics), lineY);
        alphaskia_canvas_line_to(canvas, alphaskia_text_metrics_get_actual_bounding_box_right(text_metrics), y);
        alphaskia_canvas_stroke(canvas);

        const float widthLineY = aboveAlphabetic ? lineY - 5 : lineY + 5;
        alphaskia_canvas_begin_path(canvas);
        alphaskia_canvas_move_to(canvas, 0, widthLineY);
        alphaskia_canvas_line_to(canvas, alphaskia_text_metrics_get_width(text_metrics), widthLineY);
        alphaskia_canvas_stroke(canvas);

        alphaskia_text_metrics_free(text_metrics);
    }
}

bool isDebug()
{
    return getenv("ALPHASKIA_TEST_DEBUG");
}

int measure_test(const std::filesystem::path &repository_root, const std::filesystem::path &test_data_path, bool isFreeType)
{
    std::cout << "BEGIN: measure_test" << std::endl;

    // https://jsfiddle.net/danielku15/j23dn09x/
    /*
async function run() {
  const notoSans =
    "https://cdn.jsdelivr.net/gh/notofonts/notofonts.github.io/fonts/NotoSans/unhinted/otf/NotoSans-Regular.otf"
  const font = new FontFace("Noto Sans", `url(${notoSans})`)
  document.fonts.add(font)
  await font.load()

  const canvas = document.createElement("canvas")
  canvas.width = 550
  canvas.height = 500
  document.body.appendChild(canvas)

  const ctx = canvas.getContext("2d")
  ctx.font = '18px "Noto Sans"'
  ctx.strokeStyle = "red"

  const baselinesAboveAlphabetic = [
    "fontBoundingBoxAscent",
    "actualBoundingBoxAscent",
    "emHeightAscent",
    "hangingBaseline"
  ]
  const baselinesBelowAlphabetic = [
    "ideographicBaseline",
    "emHeightDescent",
    "actualBoundingBoxDescent",
    "fontBoundingBoxDescent",
    "alphabeticBaseline"
  ]
  const baselines = [...baselinesAboveAlphabetic, ...baselinesBelowAlphabetic]
  baselines.forEach((baseline, index) => {
    const text = `Abcdefghijklmnop (${baseline})`
    const textMetrics = ctx.measureText(text)

    console.log(text);
    console.log(`  get_actual_bounding_box_ascent: ${textMetrics.actualBoundingBoxAscent}`);
    console.log(`  get_actual_bounding_box_descent: ${textMetrics.actualBoundingBoxDescent}`);
    console.log(`  get_actual_bounding_box_left: ${textMetrics.actualBoundingBoxLeft}`);
    console.log(`  get_actual_bounding_box_right: ${textMetrics.actualBoundingBoxRight}`);
    console.log(`  get_alphabetic_baseline: ${textMetrics.alphabeticBaseline}`);
    console.log(`  get_font_bounding_box_ascent: ${textMetrics.fontBoundingBoxAscent}`);
    console.log(`  get_font_bounding_box_descent: ${textMetrics.fontBoundingBoxDescent}`);
    console.log(`  get_ideographic_baseline: ${textMetrics.ideographicBaseline}`);
    console.log(`  get_hanging_baseline: ${textMetrics.hangingBaseline}`);
    console.log(`  get_width: ${textMetrics.width}`);
    console.log(`  get_em_height_ascent: ${textMetrics.emHeightAscent}`);
    console.log(`  get_em_height_descent: ${textMetrics.emHeightDescent}`);

    const y = 50 + index * 50
    ctx.fillText(text, 0, y)

    const baselineMetricValue = textMetrics[baseline]
    if (baselineMetricValue === undefined) {
      return
    }

    const lineY = baselinesBelowAlphabetic.includes(baseline)
      ? y + Math.abs(baselineMetricValue)
      : y - Math.abs(baselineMetricValue)
    ctx.beginPath()
        ctx.moveTo(textMetrics.actualBoundingBoxLeft, y)
    ctx.lineTo(textMetrics.actualBoundingBoxLeft, lineY)
    ctx.lineTo(textMetrics.actualBoundingBoxRight, lineY)
    ctx.lineTo(textMetrics.actualBoundingBoxRight, y)
    ctx.stroke()

    const widthLineY = baselinesBelowAlphabetic.includes(baseline)
      ? y + Math.abs(baselineMetricValue) + 5
      : y - Math.abs(baselineMetricValue) - 5

    ctx.beginPath()
    ctx.lineTo(0, widthLineY)
    ctx.lineTo(textMetrics.width, widthLineY)
    ctx.stroke()
})
}

run();
    */
    auto canvas = alphaskia_canvas_new();

    alphaskia_canvas_begin_render(canvas, 550, 500, 1);

    std::vector<const char *> familyNamesSerif({"Noto Serif"});
    auto text_style_serif = alphaskia_text_style_new(
        familyNamesSerif.size(),
        const_cast<const char **>(familyNamesSerif.data()),
        400,
        1);

    auto tuning1 = alphaskia_canvas_measure_text(
        canvas,
        u"Guitar Standard Tuning",
        22,
        text_style_serif,
        12,
        alphaskia_text_align_left,
        alphaskia_text_baseline_alphabetic);

    std::cout << "1: Guitar Standard Tuning " << alphaskia_text_metrics_get_actual_bounding_box_ascent(tuning1) << std::endl;

    auto tuning2 = alphaskia_canvas_measure_text(
        canvas,
        u"Guitar Standard Tuning",
        22,
        text_style_serif,
        12,
        alphaskia_text_align_left,
        alphaskia_text_baseline_alphabetic);
    std::cout << "2: Guitar Standard Tuning " << alphaskia_text_metrics_get_actual_bounding_box_ascent(tuning2) << std::endl;

    std::vector<const char *> familyNames({"Noto Sans"});
    auto text_style = alphaskia_text_style_new(
        familyNames.size(),
        const_cast<const char **>(familyNames.data()),
        400,
        0);

    std::vector<std::u16string> baselines({u"fontBoundingBoxAscent",
                                           u"actualBoundingBoxAscent",
                                           u"emHeightAscent",
                                           u"hangingBaseline",
                                           u"ideographicBaseline",
                                           u"emHeightDescent",
                                           u"actualBoundingBoxDescent",
                                           u"fontBoundingBoxDescent",
                                           u"alphabeticBaseline"});

    size_t index = 0;

    std::cout << "FAILED: measure_test, empty text" << std::endl;
    auto empty_text = alphaskia_canvas_measure_text(
        canvas,
        u"",
        0,
        text_style,
        18,
        alphaskia_text_align_left,
        alphaskia_text_baseline_alphabetic);
    if (!empty_text)
    {
        std::cout << "FAILED: measure_test, empty text" << std::endl;
        return false;
    }
    alphaskia_text_metrics_free(empty_text);

    measure_text_draw(canvas, text_style, baselines, index);

    alphaskia_image_t actual_image = alphaskia_canvas_end_render(canvas);

    alphaskia_canvas_free(canvas);

    if (!reference_image_test(repository_root, test_data_path, isFreeType, actual_image, "measure-"))
    {
        std::cout << "FAILED: measure_test" << std::endl;
        return false;
    }

    std::cout << "SUCCESS: measure_test" << std::endl;
    return true;
}

int main(int argc, char **argv)
{
    bool isFreeType = false;

    for (int i = 0; i < argc; i++)
    {
        if (strcmp(argv[i], "--freetype") == 0)
        {
            isFreeType = true;
            std::cout << "Switching to FreeType Fonts" << std::endl;
            alphaskia_switch_to_freetype_fonts();
        }
    }

    bool found(false);
    std::filesystem::path repository_root = find_repository_root(std::filesystem::current_path(), found);
    if (!found)
    {
        return 1;
    }

    if (isDebug())
    {
        std::cout << "Hit [ENTER] to continue, attach debugger now" << std::endl;
        std::string line;
        std::getline(std::cin, line);
    }

    // Load all fonts for rendering
    std::cout << "Loading fonts" << std::endl;
    std::filesystem::path test_data_path = repository_root / "test" / "test-data";
    auto music_typeface = alphaskia_load_typeface((test_data_path / "font" / "bravura" / "Bravura.otf").generic_string());
    auto music_typeface_name = alphaskia_typeface_get_family_name(music_typeface);
    auto music_typeface_weight = alphaskia_typeface_get_weight(music_typeface);
    auto music_typeface_italic = alphaskia_typeface_is_italic(music_typeface);
    auto music_typeface_name_raw = alphaskia_string_get_utf8(music_typeface_name);
    music_text_style = alphaskia_text_style_new(1, &music_typeface_name_raw, music_typeface_weight, music_typeface_italic);
    alphaskia_string_free(music_typeface_name);

    alphaskia_load_typeface((test_data_path / "font" / "noto-sans" / "NotoSans-Regular.otf").generic_string());
    alphaskia_load_typeface((test_data_path / "font" / "noto-sans" / "NotoSans-Bold.otf").generic_string());
    alphaskia_load_typeface((test_data_path / "font" / "noto-sans" / "NotoSans-Italic.otf").generic_string());
    alphaskia_load_typeface((test_data_path / "font" / "noto-sans" / "NotoSans-BoldItalic.otf").generic_string());
    alphaskia_load_typeface((test_data_path / "font" / "noto-serif" / "NotoSerif-Regular.otf").generic_string());
    alphaskia_load_typeface((test_data_path / "font" / "noto-serif" / "NotoSerif-Bold.otf").generic_string());
    alphaskia_load_typeface((test_data_path / "font" / "noto-serif" / "NotoSerif-Italic.otf").generic_string());
    alphaskia_load_typeface((test_data_path / "font" / "noto-serif" / "NotoSerif-BoldItalic.otf").generic_string());
    alphaskia_load_typeface((test_data_path / "font" / "noto-serif" / "NotoSerif-BoldItalic.otf").generic_string());
    alphaskia_load_typeface((test_data_path / "font" / "noto-music" / "NotoMusic-Regular.otf").generic_string());
    alphaskia_load_typeface((test_data_path / "font" / "noto-color-emoji" / "NotoColorEmoji_WindowsCompatible.ttf").generic_string());

    std::cout << "Fonts loaded" << std::endl;

    bool error = false;
    if (!music_sheet_test(repository_root, test_data_path, isFreeType))
    {
        error = true;
    }

    if (!empty_image_test())
    {
        error = true;
    }

    if (!measure_test(repository_root, test_data_path, isFreeType))
    {
        error = true;
    }

    return error ? 1 : 0;
}