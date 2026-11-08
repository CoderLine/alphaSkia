#pragma once

#include <filesystem>

// Verifies HTML5 canvas text semantics (as implemented by Chromium) for fill_text and measure_text,
// including text mixing glyphs of several fonts. Writes an overview image for visual review to overview_output_file.
bool text_rendering_test(const std::filesystem::path &overview_output_file);
