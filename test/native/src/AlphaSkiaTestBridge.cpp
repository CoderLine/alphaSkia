#include "../include/AlphaSkiaTestBridge.h"

#include <map>
#include <fstream>
#include <vector>
#include <iostream>

alphaskia_text_align_t text_align = alphaskia_text_align_left;
alphaskia_text_baseline_t text_baseline = alphaskia_text_baseline_top;
alphaskia_typeface_t typeface = nullptr;
alphaskia_typeface_t music_typeface = nullptr;
float music_font_size = 34;
float render_scale = 1;
float font_size = 0.0f;

static std::map<std::string, alphaskia_typeface_t> custom_type_faces;

std::string custom_typeface_key(const char *name, bool is_bold, bool is_italic)
{
    return std::string(name) + "_" + (is_bold ? "true" : "false") + "_" + (is_italic ? "true" : "false");
}

alphaskia_typeface_t alphaskia_get_typeface(const char *name, bool is_bold, bool is_italic)
{
    auto key = custom_typeface_key(name, is_bold, is_italic);
    auto it = custom_type_faces.find(key);
    if (it == custom_type_faces.end())
    {
        throw std::exception(("Unknown font requested: " + key).c_str());
    }
    return it->second;
}

void read_file(std::string file_path, std::vector<uint8_t> &data)
{
    std::ifstream in(file_path, std::ios::binary | std::ios::ate);
    if (!in)
    {
        throw std::exception(("Could not open file " + file_path).c_str());
    }

    std::streampos size = in.tellg();
    data.resize(size);
    in.seekg(0);
    in.read(reinterpret_cast<char *>(data.data()), size);
}

alphaskia_typeface_t alphaskia_load_typeface(const char *name, bool is_bold, bool is_italic, std::string file_path)
{
    auto key = custom_typeface_key(name, is_bold, is_italic);
    std::cout << "Loading font " << key << " from " << file_path << std::endl;

    std::vector<uint8_t> data;
    read_file(file_path, data);
    alphaskia_data_t font_data = alphaskia_data_new_copy(data.data(), data.size());
    if (!font_data)
    {
        throw std::exception(("Could allocate graphic font data " + file_path).c_str());
    }

    std::cout << "Read " << data.size() << " bytes from file, decoding font" << std::endl;
    alphaskia_typeface_t typeface = alphaskia_typeface_register(font_data);
    if (!typeface)
    {
        alphaskia_data_free(font_data);
        throw std::exception("Could not create font from data");
    }
    std::cout << "Font " << key << " loaded and registered" << std::endl;
    custom_type_faces[key] = typeface;
    return typeface;
}