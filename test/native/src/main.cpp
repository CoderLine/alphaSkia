#include "../include/AlphaTabGeneratedTest.h"
#include "../include/PixelMatch.h"

#include <filesystem>
#include <iostream>
#include <fstream>

#ifndef ALPHASKIA_RID
#error "Missing alphaSkia runtime identifier, please specify ALPHASKIA_RID";
#endif

#define STRINGIFY(s) _STRINGIFY(s)
#define _STRINGIFY(s) #s

std::filesystem::path find_repository_root(std::filesystem::path current, bool &success)
{
    if (std::filesystem::exists(current / ".nuke"))
    {
        std::cerr << "Repository root found at " << current << std::endl;
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

int main()
{
    const double tolerance_percent = 1;
    bool found(false);
    std::filesystem::path repository_root = find_repository_root(std::filesystem::current_path(), found);
    if (!found)
    {
        return 1;
    }

    // Load all fonts for rendering
    std::cout << "Loading fonts" << std::endl;
    std::filesystem::path test_data_path = repository_root / "test" / "test-data";
    music_typeface = alphaskia_load_typeface("Bravura", false, false, (test_data_path / "font" / "bravura" / "Bravura.ttf").generic_string());
    alphaskia_load_typeface("Roboto", false, false, (test_data_path / "font" / "roboto" / "Roboto-Regular.ttf").generic_string());
    alphaskia_load_typeface("Roboto", true, false, (test_data_path / "font" / "roboto" / "Roboto-Bold.ttf").generic_string());
    alphaskia_load_typeface("Roboto", false, true, (test_data_path / "font" / "roboto" / "Roboto-Italic.ttf").generic_string());
    alphaskia_load_typeface("Roboto", true, true, (test_data_path / "font" / "roboto" / "Roboto-BoldItalic.ttf").generic_string());
    alphaskia_load_typeface("PT Serif", false, false, (test_data_path / "font" / "ptserif" / "PTSerif-Regular.ttf").generic_string());
    alphaskia_load_typeface("PT Serif", true, false, (test_data_path / "font" / "ptserif" / "PTSerif-Bold.ttf").generic_string());
    alphaskia_load_typeface("PT Serif", false, true, (test_data_path / "font" / "ptserif" / "PTSerif-Italic.ttf").generic_string());
    alphaskia_load_typeface("PT Serif", true, true, (test_data_path / "font" / "ptserif" / "PTSerif-BoldItalic.ttf").generic_string());
    std::cout << "Fonts loaded" << std::endl;

    // render full image
    std::cout << "Rendering image" << std::endl;
    alphaskia_image_t actual_image = render_full_image();
    std::cout << "Image rendered" << std::endl;

    // save png for future reference
    std::cout << "Saving image as PNG" << std::endl;

    std::filesystem::path test_output_path = repository_root / "test" / "test-outputs" / "native";
    std::filesystem::create_directories(test_output_path);

    std::filesystem::path test_output_file = test_output_path / (std::string(STRINGIFY(ALPHASKIA_RID)) + ".png");
    alphaskia_data_t png_data = alphaskia_image_encode_png(actual_image);
    if (!png_data)
    {
        std::cerr << "Failed to encode final image to png" << std::endl;
        return 1;
    }
    write_data_to_file_and_free(png_data, test_output_file.generic_string());
    std::cout << "Image saved to " << test_output_file.generic_string() << std::endl;

    // load reference image
    std::filesystem::path test_reference_path = test_data_path / "reference" / (std::string(STRINGIFY(ALPHASKIA_RID)) + ".png");
    std::cout << "Loading reference image " << test_reference_path.generic_string() << std::endl;
    std::vector<uint8_t> test_reference_data;
    read_file(test_reference_path.generic_string(), test_reference_data);
    alphaskia_image_t expected_image = alphaskia_image_decode(test_reference_data.data(), test_reference_data.size());
    if (!expected_image)
    {
        std::cerr << "Failed to decode reference image" << std::endl;
        return 1;
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
        return 1;
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

        auto diff_output_path = test_output_path / (std::string(STRINGIFY(ALPHASKIA_RID)) + ".diff.png");
        write_data_to_file_and_free(diff_image_png_data, diff_output_path.generic_string());
        std::cout << "Error diff image saved to " << diff_output_path.generic_string() << std::endl;
        return 1;
    }

    std::cout << "Images match. Total Pixels: " << compare_result.total_pixels << ", Transparent Pixels: " << compare_result.transparent_pixels << ", Percent difference:" << percentDifference << "%" << std::endl;
    return 0;
}