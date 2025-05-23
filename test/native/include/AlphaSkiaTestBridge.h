#pragma once

#include "../../../wrapper/include/alphaskia.h"

#include <stdexcept>
#include <functional>
#include <memory>
#include <cstdint>
#include <array>
#include <string>

#define alphaskia_rgba_to_color(r, g, b, a) static_cast<uint32_t>(((a & 0xFF) << 24) | ((r & 0xFF) << 16) | ((g & 0xFF) << 8) | ((b & 0xFF) << 0))

typedef std::function<alphaskia_image_t(alphaskia_canvas_t)> render_function_t;

extern alphaskia_text_align_t text_align;
extern alphaskia_text_baseline_t text_baseline;
extern alphaskia_text_style_t text_style;
extern alphaskia_text_style_t music_text_style;
extern float music_font_size;
extern float font_size;
extern float render_scale;
extern int32_t total_width;
extern int32_t total_height;

alphaskia_text_style_t alphaskia_get_text_style(const std::vector<const char*>& family_names, uint16_t weight, bool is_italic);
alphaskia_typeface_t alphaskia_load_typeface(std::string file_path);
void read_file(std::string file_path, std::vector<uint8_t> &data);