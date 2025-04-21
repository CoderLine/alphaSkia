#include "../include/AlphaSkiaTestBridge.h"

#include <map>
#include <fstream>
#include <vector>
#include <iostream>
#include <sstream>

alphaskia_text_align_t text_align = alphaskia_text_align_left;
alphaskia_text_baseline_t text_baseline = alphaskia_text_baseline_top;
alphaskia_textstyle_t text_style = nullptr;
alphaskia_textstyle_t music_text_style = nullptr;
float music_font_size = 34;
float render_scale = 1;
float font_size = 12.0f;

static std::map<std::string, alphaskia_textstyle_t> custom_text_styles;

std::string custom_textstyle_key(const std::vector<const char *> &family_names, uint16_t weight, bool is_italic)
{
    std::stringstream s;
    for (auto &name : family_names)
    {
        s << name << "_";
    }
    s << weight;
    s << (is_italic ? "italic" : "upright");

    return s.str();
}

alphaskia_textstyle_t alphaskia_get_text_style(const std::vector<const char *> &family_names, uint16_t weight, bool is_italic)
{
    auto key = custom_textstyle_key(family_names, weight, is_italic);
    auto it = custom_text_styles.find(key);
    if (it == custom_text_styles.end())
    {
        alphaskia_textstyle_t new_text_style = alphaskia_textstyle_new(
            static_cast<uint8_t>(family_names.size()),
            const_cast<const char **>(&family_names[0]),
            weight,
            is_italic);
        custom_text_styles[key] = new_text_style;

        return new_text_style;
    }
    return it->second;
}

void read_file(std::string file_path, std::vector<uint8_t> &data)
{
    std::ifstream in(file_path, std::ios::binary | std::ios::ate);
    if (!in)
    {
        std::cerr << "Could not open file " << file_path << std::endl;
        return;
    }

    std::streampos size = in.tellg();
    data.resize(size);
    in.seekg(0);
    in.read(reinterpret_cast<char *>(data.data()), size);
}

alphaskia_typeface_t alphaskia_load_typeface(std::string file_path)
{
    std::cout << "Loading typeface from " << file_path << std::endl;

    std::vector<uint8_t> data;
    read_file(file_path, data);
    alphaskia_data_t font_data = alphaskia_data_new_copy(data.data(), data.size());
    if (!font_data)
    {
        std::cerr << "Could allocate typeface data " << file_path << std::endl;
        return nullptr;
    }

    std::cout << "Read " << data.size() << " bytes from file, decoding typeface" << std::endl;
    alphaskia_typeface_t typeface = alphaskia_typeface_register(font_data);
    if (!typeface)
    {
        alphaskia_data_free(font_data);
        std::cerr << "Could not create typeface from data " << file_path << std::endl;
        return nullptr;
    }
    std::cout << "Typeface "
              << alphaskia_typeface_get_family_name(typeface)
              << "weight: " << alphaskia_typeface_get_weight(typeface)
              << " italic: " << (alphaskia_typeface_is_italic(typeface) ? "yes" : "no")
              << " loaded" << std::endl;
    return typeface;
}