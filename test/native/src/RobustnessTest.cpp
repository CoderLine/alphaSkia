#include "../include/RobustnessTest.h"
#include "../include/AlphaSkiaTestBridge.h"

#include <atomic>
#include <cmath>
#include <iostream>
#include <limits>
#include <string>
#include <thread>
#include <vector>

namespace
{
    int checks = 0;
    int failures = 0;
    alphaskia_text_style_t style = nullptr;

    void check(bool condition, const std::string &message)
    {
        checks++;
        if (!condition)
        {
            failures++;
            std::cerr << "  FAILED: " << message << std::endl;
        }
    }

    const std::vector<uint8_t> garbage = {0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0xFF, 0xFE};

    void test_invalid_fonts()
    {
        auto garbage_data = alphaskia_data_new_copy(garbage.data(), garbage.size());
        check(alphaskia_typeface_register(garbage_data) == nullptr, "typeface from garbage bytes");
        alphaskia_data_free(garbage_data);

        auto empty_data = alphaskia_data_new_copy(nullptr, 0);
        check(empty_data != nullptr, "empty data");
        check(alphaskia_typeface_register(empty_data) == nullptr, "typeface from empty data");
        alphaskia_data_free(empty_data);

        check(alphaskia_data_new_copy(nullptr, 5) == nullptr, "data from NULL with length");
        check(alphaskia_typeface_register(nullptr) == nullptr, "typeface from NULL data");
    }

    void test_invalid_images()
    {
        check(alphaskia_image_decode(garbage.data(), garbage.size()) == nullptr, "decode garbage bytes");
        check(alphaskia_image_decode(nullptr, 0) == nullptr, "decode empty");
        check(alphaskia_image_decode(nullptr, 5) == nullptr, "decode NULL with length");

        std::vector<uint8_t> pixels(16);
        check(alphaskia_image_from_pixels(-1, 1, pixels.data(), pixels.size()) == nullptr, "pixels with negative width");
        check(alphaskia_image_from_pixels(1, 0, pixels.data(), pixels.size()) == nullptr, "pixels with zero height");
        check(alphaskia_image_from_pixels(1, 1, nullptr, 4) == nullptr, "pixels from NULL");
        check(alphaskia_image_from_pixels(1000, 1000, pixels.data(), pixels.size()) == nullptr, "pixels larger than the buffer");
        check(alphaskia_image_from_pixels(65536, 65536, pixels.data(), pixels.size()) == nullptr, "pixels with overflowing 32-bit size");

        auto image = alphaskia_image_from_pixels(2, 2, pixels.data(), pixels.size());
        check(image != nullptr, "pixels 2x2");
        check(alphaskia_image_read_pixels(image, nullptr, 8, 16) == 0, "read pixels into NULL");
        check(alphaskia_image_read_pixels(image, pixels.data(), 8, 15) == 0, "read pixels into a too small buffer");
        check(alphaskia_image_read_pixels(image, pixels.data(), 4, 16) == 0, "read pixels with too small row bytes");
        check(alphaskia_image_read_pixels(image, pixels.data(), 8, 16) == 1, "read pixels into an exact buffer");
        std::vector<uint8_t> padded(20);
        check(alphaskia_image_read_pixels(image, padded.data(), 12, padded.size()) == 1, "read pixels with padded rows");
        alphaskia_image_free(image);
    }

    void test_null_handles()
    {
        check(alphaskia_image_get_width(nullptr) == 0, "image width of NULL");
        check(alphaskia_image_get_height(nullptr) == 0, "image height of NULL");
        uint8_t pixel[4];
        check(alphaskia_image_read_pixels(nullptr, pixel, 4, sizeof(pixel)) == 0, "read pixels of NULL");
        check(alphaskia_image_encode_png(nullptr) == nullptr, "encode NULL");

        check(alphaskia_data_get_data(nullptr) == nullptr, "data of NULL");
        check(alphaskia_data_get_length(nullptr) == 0, "data length of NULL");
        check(alphaskia_string_get_utf8(nullptr) == nullptr, "string of NULL");
        check(alphaskia_string_get_length(nullptr) == 0, "string length of NULL");

        check(alphaskia_typeface_get_family_name(nullptr) == nullptr, "family name of NULL typeface");
        check(alphaskia_typeface_get_weight(nullptr) == 0, "weight of NULL typeface");
        check(alphaskia_typeface_is_italic(nullptr) == 0, "italic of NULL typeface");

        check(alphaskia_text_style_new(2, nullptr, 400, 0) == nullptr, "text style with NULL families");
        check(alphaskia_text_style_get_family_name_count(nullptr) == 0, "family count of NULL text style");
        check(alphaskia_text_style_get_family_name(nullptr, 0) == nullptr, "family of NULL text style");
        check(alphaskia_text_style_get_weight(nullptr) == 0, "weight of NULL text style");

        check(alphaskia_text_metrics_get_width(nullptr) == 0, "width of NULL text metrics");

        check(alphaskia_canvas_end_render(nullptr) == nullptr, "end render of NULL canvas");
        check(alphaskia_canvas_get_color(nullptr) == 0, "color of NULL canvas");
        alphaskia_canvas_fill_rect(nullptr, 0, 0, 1, 1);
        alphaskia_canvas_fill_text(nullptr, u"a", 1, style, 12, 0, 0, alphaskia_text_align_left, alphaskia_text_baseline_alphabetic);

        alphaskia_image_free(nullptr);
        alphaskia_data_free(nullptr);
        alphaskia_string_free(nullptr);
        alphaskia_typeface_free(nullptr);
        alphaskia_text_style_free(nullptr);
        alphaskia_text_metrics_free(nullptr);
        alphaskia_canvas_free(nullptr);
    }

    void draw_everything(alphaskia_canvas_t canvas)
    {
        alphaskia_canvas_fill_rect(canvas, 0, 0, 10, 10);
        alphaskia_canvas_stroke_rect(canvas, 0, 0, 10, 10);
        alphaskia_canvas_begin_path(canvas);
        alphaskia_canvas_move_to(canvas, 0, 0);
        alphaskia_canvas_line_to(canvas, 10, 10);
        alphaskia_canvas_fill(canvas);
        alphaskia_canvas_stroke_circle(canvas, 5, 5, 5);
        alphaskia_canvas_begin_rotate(canvas, 5, 5, 45);
        alphaskia_canvas_end_rotate(canvas);
        alphaskia_canvas_fill_text(canvas, u"a", 1, style, 12, 0, 10, alphaskia_text_align_left, alphaskia_text_baseline_alphabetic);
    }

    void test_canvas_without_surface()
    {
        auto canvas = alphaskia_canvas_new();

        draw_everything(canvas);
        check(alphaskia_canvas_end_render(canvas) == nullptr, "end render without begin render");

        const float invalid_scales[] = {0.0f, -1.0f, std::numeric_limits<float>::quiet_NaN(), std::numeric_limits<float>::infinity()};
        for (float scale : invalid_scales)
        {
            check(alphaskia_canvas_begin_render(canvas, 10, 10, scale) == 0, "begin render with scale " + std::to_string(scale));
            draw_everything(canvas);
            check(alphaskia_canvas_end_render(canvas) == nullptr, "end render after begin render with scale " + std::to_string(scale));
        }

        check(alphaskia_canvas_begin_render(canvas, std::numeric_limits<int32_t>::max(), 10, 2.0f) == 0, "begin render with overflowing width");
        check(alphaskia_canvas_end_render(canvas) == nullptr, "end render after begin render with overflowing width");

        check(alphaskia_canvas_begin_render(canvas, 10, 10, 1.0f) == 1, "begin render with valid size");
        draw_everything(canvas);
        auto image = alphaskia_canvas_end_render(canvas);
        check(image != nullptr && alphaskia_image_get_width(image) == 10, "end render after valid begin render");
        alphaskia_image_free(image);

        alphaskia_canvas_free(canvas);
    }

    void test_invalid_text()
    {
        auto canvas = alphaskia_canvas_new();
        alphaskia_canvas_begin_render(canvas, 10, 10, 1.0f);

        const auto align = alphaskia_text_align_left;
        const auto baseline = alphaskia_text_baseline_alphabetic;
        check(alphaskia_canvas_measure_text(canvas, u"a", -1, style, 12, align, baseline) == nullptr, "measure negative length");
        check(alphaskia_canvas_measure_text(canvas, nullptr, 3, style, 12, align, baseline) == nullptr, "measure NULL text with length");
        check(alphaskia_canvas_measure_text(canvas, u"a", 1, nullptr, 12, align, baseline) == nullptr, "measure NULL text style");
        check(alphaskia_canvas_measure_text(canvas, u"a", 1, style, std::numeric_limits<float>::infinity(), align, baseline) == nullptr, "measure infinite font size");
        check(alphaskia_canvas_measure_text(canvas, u"a", 1, style, std::numeric_limits<float>::quiet_NaN(), align, baseline) == nullptr, "measure NaN font size");

        alphaskia_canvas_fill_text(canvas, u"a", -1, style, 12, 0, 0, align, baseline);
        alphaskia_canvas_fill_text(canvas, nullptr, 3, style, 12, 0, 0, align, baseline);
        alphaskia_canvas_fill_text(canvas, u"a", 1, nullptr, 12, 0, 0, align, baseline);

        auto empty = alphaskia_canvas_measure_text(canvas, nullptr, 0, style, 12, align, baseline);
        check(empty != nullptr && alphaskia_text_metrics_get_width(empty) == 0, "measure NULL text without length");
        alphaskia_text_metrics_free(empty);

        auto huge = alphaskia_canvas_measure_text(canvas, u"a", 1, style, 1e30f, align, alphaskia_text_baseline_middle);
        check(huge != nullptr, "measure huge font size");
        alphaskia_text_metrics_free(huge);

        auto paired = alphaskia_canvas_measure_text(canvas, u"A\U0001D11EB", 4, style, 12, align, baseline);
        auto unpaired_high = alphaskia_canvas_measure_text(canvas, u"A\xD834" u"B", 3, style, 12, align, baseline);
        auto unpaired_low = alphaskia_canvas_measure_text(canvas, u"A\xDD1E" u"B", 3, style, 12, align, baseline);
        auto letters = alphaskia_canvas_measure_text(canvas, u"AB", 2, style, 12, align, baseline);
        const float letters_width = alphaskia_text_metrics_get_width(letters);
        check(alphaskia_text_metrics_get_width(paired) > letters_width, "surrogate pair is measured");
        check(alphaskia_text_metrics_get_width(unpaired_high) > letters_width, "unpaired high surrogate keeps the text");
        check(alphaskia_text_metrics_get_width(unpaired_low) > letters_width, "unpaired low surrogate keeps the text");
        alphaskia_text_metrics_free(paired);
        alphaskia_text_metrics_free(unpaired_high);
        alphaskia_text_metrics_free(unpaired_low);
        alphaskia_text_metrics_free(letters);

        alphaskia_image_free(alphaskia_canvas_end_render(canvas));
        alphaskia_canvas_free(canvas);
    }
}

namespace
{
    void test_concurrent_font_use(const std::filesystem::path &test_data_path)
    {
        std::vector<uint8_t> font_bytes;
        read_file((test_data_path / "font" / "noto-sans" / "NotoSans-Regular.otf").generic_string(), font_bytes);

        std::atomic<int> failed_measurements(0);
        std::vector<std::thread> threads;
        for (int t = 0; t < 4; t++)
        {
            threads.emplace_back([&]()
                                 {
                auto canvas = alphaskia_canvas_new();
                for (int i = 0; i < 20; i++)
                {
                    auto data = alphaskia_data_new_copy(font_bytes.data(), font_bytes.size());
                    alphaskia_typeface_free(alphaskia_typeface_register(data));
                    alphaskia_data_free(data);
                    alphaskia_typeface_free(alphaskia_typeface_make_from_name("Noto Sans", 400, 0));

                    for (int j = 0; j < 5; j++)
                    {
                        auto metrics = alphaskia_canvas_measure_text(canvas, u"Concurrent", 10, style, 12, alphaskia_text_align_left, alphaskia_text_baseline_alphabetic);
                        if (!metrics || alphaskia_text_metrics_get_width(metrics) <= 0)
                        {
                            failed_measurements++;
                        }
                        alphaskia_text_metrics_free(metrics);
                    }
                }
                alphaskia_canvas_free(canvas); });
        }
        for (auto &thread : threads)
        {
            thread.join();
        }
        check(failed_measurements == 0, std::to_string(failed_measurements) + " concurrent measurements failed");
    }
}

bool robustness_test(const std::filesystem::path &test_data_path)
{
    std::cout << "BEGIN: robustness_test" << std::endl;
    style = alphaskia_get_text_style({"Noto Sans", "Noto Music"}, 400, false);

    test_invalid_fonts();
    test_invalid_images();
    test_null_handles();
    test_canvas_without_surface();
    test_invalid_text();
    test_concurrent_font_use(test_data_path);

    alphaskia_text_style_free(style);

    std::cout << "  " << (checks - failures) << "/" << checks << " checks passed" << std::endl;
    if (failures > 0)
    {
        std::cout << "FAILED: robustness_test" << std::endl;
        return false;
    }

    std::cout << "SUCCESS: robustness_test" << std::endl;
    return true;
}
