#include "../../../externals/node-api-headers/include/node_api.h"
#include "../../../wrapper/include/alphaskia.h"

#include <assert.h>
#include <vector>
#include <string>
#include <cstring>
#include <memory>

#define CONCAT(a, b) a##b
#define STRINGIFY(s) _STRINGIFY(s)
#define _STRINGIFY(s) #s
#define ASSERT_STATUS()                                \
  if (status != napi_ok)                               \
  {                                                    \
    napi_throw_error(env, "status", "Internal error"); \
    return nullptr;                                    \
  }

static const napi_type_tag alphaskia_data_t_tag = {0x6a960ece6a0c4caf, 0xad61688dc0a28531};
static const napi_type_tag alphaskia_typeface_t_tag = {0x0068df0314224b96, 0x8048b968915995f1};
static const napi_type_tag alphaskia_image_t_tag = {0x9372c0f8e8de466f, 0x96a04b6ee0a9394b};
static const napi_type_tag alphaskia_canvas_t_tag = {0xaa77c76772a34052, 0x88ac80d4dc474395};
static const napi_type_tag alphaskia_text_style_t_tag = {0x8f2bc41a57024cf4, 0x8b5e1cd26ded5245};
static const napi_type_tag alphaskia_text_metrics_t_tag = {0x85fbe8d4da634b51, 0x970a89efa0ef13ff};

#define RETURN_UNDEFINED()                        \
  {                                               \
    napi_value undefined;                         \
    status = napi_get_undefined(env, &undefined); \
    ASSERT_STATUS();                              \
    return undefined;                             \
  }

#define CHECK_ARGUMENT_TYPE(index, expected)                                                                                     \
  {                                                                                                                              \
    napi_valuetype actual;                                                                                                       \
    status = napi_typeof(env, node_argv[index], &actual);                                                                        \
    ASSERT_STATUS();                                                                                                             \
    if (actual != expected)                                                                                                      \
    {                                                                                                                            \
      napi_throw_type_error(env, NULL, "Wrong argument type at index " STRINGIFY(index) ", expected type " STRINGIFY(expected)); \
      return nullptr;                                                                                                            \
    }                                                                                                                            \
  }

#define CHECK_ARGUMENT_IS_ARRAYBUFFER(index)                                                                       \
  {                                                                                                                \
    bool is_buffer(false);                                                                                         \
    status = napi_is_arraybuffer(env, node_argv[index], &is_buffer);                                               \
    ASSERT_STATUS();                                                                                               \
    if (!is_buffer)                                                                                                \
    {                                                                                                              \
      napi_throw_type_error(env, NULL, "Wrong argument type at index " STRINGIFY(index) ", expected ArrayBuffer"); \
      return nullptr;                                                                                              \
    }                                                                                                              \
  }

#define CHECK_ARGS(count)                                                        \
  size_t node_argc = count;                                                      \
  napi_value node_argv[count];                                                   \
  status = napi_get_cb_info(env, info, &node_argc, node_argv, nullptr, nullptr); \
  ASSERT_STATUS();                                                               \
  if (node_argc != count)                                                        \
  {                                                                              \
    napi_throw_type_error(env, NULL, "Wrong number of arguments");               \
    return nullptr;                                                              \
  }

#define WRAP_HANDLE(type, name, value)                                                                                              \
  napi_value name;                                                                                                                  \
  if (value)                                                                                                                        \
  {                                                                                                                                 \
    status = napi_create_object(env, &name);                                                                                        \
    ASSERT_STATUS();                                                                                                                \
    status = napi_type_tag_object(env, name, &CONCAT(type, _tag));                                                                  \
    ASSERT_STATUS();                                                                                                                \
    status = napi_wrap(env, name, value, nullptr, nullptr, nullptr);                                                                \
    ASSERT_STATUS();                                                                                                                \
    napi_value public_tag_value;                                                                                                    \
    status = napi_create_string_utf8(env, STRINGIFY(type), NAPI_AUTO_LENGTH, &public_tag_value);                                    \
    ASSERT_STATUS();                                                                                                                \
    napi_property_descriptor public_tag = {/* utf8name: */ "_alphaskia",                                                            \
                                           /* name: */ nullptr,                                                                     \
                                           /* method: */ nullptr,                                                                   \
                                           /* getter: */ nullptr,                                                                   \
                                           /* setter: */ nullptr,                                                                   \
                                           /* value: */ public_tag_value,                                                           \
                                           /* attributes: */ static_cast<napi_property_attributes>(napi_default | napi_enumerable), \
                                           /* data: */ nullptr};                                                                    \
    status = napi_define_properties(env, name, 1, &public_tag);                                                                     \
    ASSERT_STATUS();                                                                                                                \
  }                                                                                                                                 \
  else                                                                                                                              \
  {                                                                                                                                 \
    status = napi_get_undefined(env, &name);                                                                                        \
    ASSERT_STATUS();                                                                                                                \
  }

#define GET_ARGUMENT_HANDLE(index, type, name)                                                                                         \
  type name;                                                                                                                           \
  {                                                                                                                                    \
    bool is_handle(false);                                                                                                             \
    status = napi_check_object_type_tag(env,                                                                                           \
                                        node_argv[index],                                                                              \
                                        &CONCAT(type, _tag),                                                                           \
                                        &is_handle);                                                                                   \
    ASSERT_STATUS();                                                                                                                   \
    if (!is_handle)                                                                                                                    \
    {                                                                                                                                  \
      napi_throw_type_error(env, NULL, "Wrong argument type at index " STRINGIFY(index) ", expected handle of type " STRINGIFY(type)); \
      return nullptr;                                                                                                                  \
    }                                                                                                                                  \
    status = napi_unwrap(env, node_argv[index], &name);                                                                                \
    ASSERT_STATUS();                                                                                                                   \
  }

#define GET_ARGUMENT_UTF8_STRING(index, name)                                                        \
  std::string name;                                                                                  \
  {                                                                                                  \
    CHECK_ARGUMENT_TYPE(index, napi_string);                                                         \
    size_t name_size;                                                                                \
    status = napi_get_value_string_utf8(env, node_argv[index], nullptr, 0, &name_size);              \
    ASSERT_STATUS();                                                                                 \
    name.assign(name_size, ' ');                                                                     \
    status = napi_get_value_string_utf8(env, node_argv[index], name.data(), name_size + 1, nullptr); \
    ASSERT_STATUS();                                                                                 \
  }

#define GET_ARGUMENT_UTF16_STRING(index, name)                                                        \
  std::u16string name;                                                                                \
  {                                                                                                   \
    CHECK_ARGUMENT_TYPE(index, napi_string);                                                          \
    size_t name_size;                                                                                 \
    status = napi_get_value_string_utf16(env, node_argv[index], nullptr, 0, &name_size);              \
    ASSERT_STATUS();                                                                                  \
    name.assign(name_size, u' ');                                                                     \
    status = napi_get_value_string_utf16(env, node_argv[index], name.data(), name_size + 1, nullptr); \
    ASSERT_STATUS();                                                                                  \
  }

#define GET_ARGUMENT_INT32(index, name)                        \
  int32_t name;                                                \
  CHECK_ARGUMENT_TYPE(index, napi_number);                     \
  status = napi_get_value_int32(env, node_argv[index], &name); \
  ASSERT_STATUS();

#define GET_ARGUMENT_UINT32(index, name)                        \
  uint32_t name;                                                \
  CHECK_ARGUMENT_TYPE(index, napi_number);                      \
  status = napi_get_value_uint32(env, node_argv[index], &name); \
  ASSERT_STATUS();

#define GET_ARGUMENT_DOUBLE(index, name)                        \
  double name;                                                  \
  CHECK_ARGUMENT_TYPE(index, napi_number);                      \
  status = napi_get_value_double(env, node_argv[index], &name); \
  ASSERT_STATUS();

#define GET_ARGUMENT_FLOAT(index, name)                             \
  float name;                                                       \
  {                                                                 \
    double _value(0);                                               \
    CHECK_ARGUMENT_TYPE(index, napi_number);                        \
    status = napi_get_value_double(env, node_argv[index], &_value); \
    ASSERT_STATUS();                                                \
    name = static_cast<float>(_value);                              \
  }
#define GET_ARGUMENT_BOOL(index, name)                        \
  bool name;                                                  \
  CHECK_ARGUMENT_TYPE(index, napi_boolean);                   \
  status = napi_get_value_bool(env, node_argv[index], &name); \
  ASSERT_STATUS();

static napi_value node_alphaskia_get_color_type(napi_env env, napi_callback_info info)
{
  napi_status status(napi_ok);

  int32_t color_type = alphaskia_get_color_type();
  napi_value node_color_type;

  status = napi_create_int32(env, color_type, &node_color_type);
  ASSERT_STATUS();

  return node_color_type;
}

static napi_value node_alphaskia_switch_to_freetype_fonts(napi_env env, napi_callback_info info)
{
  napi_status status(napi_ok);

  alphaskia_switch_to_freetype_fonts();

  RETURN_UNDEFINED();
}

static napi_value node_alphaskia_switch_to_operating_system_fonts(napi_env env, napi_callback_info info)
{
  napi_status status(napi_ok);

  alphaskia_switch_to_operating_system_fonts();

  RETURN_UNDEFINED();
}

static napi_value node_alphaskia_data_new_copy(napi_env env, napi_callback_info info)
{
  napi_status status(napi_ok);

  CHECK_ARGS(1)
  CHECK_ARGUMENT_IS_ARRAYBUFFER(0);

  void *node_data(nullptr);
  size_t node_data_length(0);
  status = napi_get_arraybuffer_info(env, node_argv[0], &node_data, &node_data_length);
  ASSERT_STATUS();

  alphaskia_data_t data = alphaskia_data_new_copy(static_cast<const uint8_t *>(node_data), node_data_length);
  WRAP_HANDLE(alphaskia_data_t, wrapped, data);
  return wrapped;
}

static napi_value node_alphaskia_data_get_data(napi_env env, napi_callback_info info)
{
  napi_status status(napi_ok);

  CHECK_ARGS(1);
  GET_ARGUMENT_HANDLE(0, alphaskia_data_t, data);

  const uint8_t *data_content = alphaskia_data_get_data(data);
  uint64_t data_length = alphaskia_data_get_length(data);

  void *node_data_content;
  napi_value node_data;
  status = napi_create_arraybuffer(env, data_length, &data, &node_data);
  ASSERT_STATUS();

  memcpy(node_data_content, data_content, static_cast<size_t>(data_length));

  return node_data;
}

static napi_value node_alphaskia_data_free(napi_env env, napi_callback_info info)
{
  napi_status status(napi_ok);

  CHECK_ARGS(1);
  GET_ARGUMENT_HANDLE(0, alphaskia_data_t, data);

  alphaskia_data_free(data);
  void *old;
  status = napi_remove_wrap(env, node_argv[0], &old);
  ASSERT_STATUS();

  RETURN_UNDEFINED();
}

static napi_value node_alphaskia_typeface_register(napi_env env, napi_callback_info info)
{
  napi_status status(napi_ok);

  CHECK_ARGS(1);
  GET_ARGUMENT_HANDLE(0, alphaskia_data_t, data);

  alphaskia_typeface_t typeface = alphaskia_typeface_register(data);
  WRAP_HANDLE(alphaskia_typeface_t, wrapped, typeface);
  return wrapped;
}

static napi_value node_alphaskia_typeface_free(napi_env env, napi_callback_info info)
{
  napi_status status(napi_ok);

  CHECK_ARGS(1);
  GET_ARGUMENT_HANDLE(0, alphaskia_typeface_t, typeface);

  alphaskia_typeface_free(typeface);
  void *old;
  status = napi_remove_wrap(env, node_argv[0], &old);
  ASSERT_STATUS();

  RETURN_UNDEFINED();
}

static napi_value node_alphaskia_typeface_make_from_name(napi_env env, napi_callback_info info)
{
  napi_status status(napi_ok);

  CHECK_ARGS(3);
  GET_ARGUMENT_UTF8_STRING(0, name);
  GET_ARGUMENT_INT32(1, weight);
  GET_ARGUMENT_BOOL(2, italic);

  alphaskia_typeface_t typeface = alphaskia_typeface_make_from_name(name.c_str(), static_cast<uint16_t>(weight), italic ? 1 : 0);
  WRAP_HANDLE(alphaskia_typeface_t, wrapped, typeface);
  return wrapped;
}

static napi_value node_alphaskia_typeface_get_family_name(napi_env env, napi_callback_info info)
{
  napi_status status(napi_ok);

  CHECK_ARGS(1);
  GET_ARGUMENT_HANDLE(0, alphaskia_typeface_t, typeface);
  alphaskia_string_t string = alphaskia_typeface_get_family_name(typeface);

  const char *utf8Bytes = alphaskia_string_get_utf8(string);
  uint64_t stringLength = alphaskia_string_get_length(string);

  napi_value family_name;
  status = napi_create_string_utf8(env, utf8Bytes, stringLength, &family_name);
  ASSERT_STATUS();

  alphaskia_string_free(string);

  return family_name;
}

static napi_value node_alphaskia_typeface_get_weight(napi_env env, napi_callback_info info)
{
  napi_status status(napi_ok);

  CHECK_ARGS(1);
  GET_ARGUMENT_HANDLE(0, alphaskia_typeface_t, typeface);
  uint16_t weight = alphaskia_typeface_get_weight(typeface);

  napi_value node_weight;
  status = napi_create_int32(env, weight, &node_weight);
  ASSERT_STATUS();

  return node_weight;
}

static napi_value node_alphaskia_typeface_is_italic(napi_env env, napi_callback_info info)
{
  napi_status status(napi_ok);

  CHECK_ARGS(1);
  GET_ARGUMENT_HANDLE(0, alphaskia_typeface_t, typeface);
  uint8_t is_italic = alphaskia_typeface_is_italic(typeface);

  napi_value node_is_italic;
  status = napi_get_boolean(env, is_italic != 0, &node_is_italic);
  ASSERT_STATUS();

  return node_is_italic;
}

static napi_value node_alphaskia_text_style_new(napi_env env, napi_callback_info info)
{
  napi_status status(napi_ok);

  CHECK_ARGS(3);

  // string[] unwrapping
  bool is_array(false);
  status = napi_is_array(env, node_argv[0], &is_array);
  ASSERT_STATUS();
  if (!is_array)
  {
    napi_throw_type_error(env, NULL, "Wrong argument type at index 0, expected array of strings");
    return nullptr;
  }

  uint32_t familyNamesLength = 0;
  status = napi_get_array_length(env, node_argv[0], &familyNamesLength);
  ASSERT_STATUS();

  familyNamesLength = static_cast<uint8_t>(familyNamesLength);

  std::vector<std::string> familyNames;
  familyNames.resize(familyNamesLength);
  std::vector<const char *> familyNamesRaw;
  familyNamesRaw.resize(familyNamesLength);

  for (uint8_t i = 0; i < familyNamesLength; i++)
  {
    napi_value item;
    status = napi_get_element(env, node_argv[0], i, &item);
    ASSERT_STATUS();

    // no need for type checking, we do that in javascript, its less code there. napi functions might still fail

    size_t name_size;
    status = napi_get_value_string_utf8(env, item, nullptr, 0, &name_size);
    ASSERT_STATUS();

    familyNames.at(i).assign(name_size, ' ');
    status = napi_get_value_string_utf8(env, item, familyNames.at(i).data(), name_size + 1, nullptr);
    ASSERT_STATUS();

    familyNamesRaw[i] = familyNames.at(i).data();
  }
  // string[] end

  GET_ARGUMENT_INT32(1, weight);
  GET_ARGUMENT_BOOL(2, italic);

  alphaskia_text_style_t text_style = alphaskia_text_style_new(static_cast<uint8_t>(familyNamesLength), &familyNamesRaw[0], static_cast<uint16_t>(weight), italic ? 1 : 0);
  WRAP_HANDLE(alphaskia_text_style_t, wrapped, text_style);
  return wrapped;
}

static napi_value node_alphaskia_text_style_free(napi_env env, napi_callback_info info)
{
  napi_status status(napi_ok);

  CHECK_ARGS(1);
  GET_ARGUMENT_HANDLE(0, alphaskia_text_style_t, text_style);

  alphaskia_text_style_free(text_style);
  void *old;
  status = napi_remove_wrap(env, node_argv[0], &old);
  ASSERT_STATUS();

  RETURN_UNDEFINED();
}

static napi_value node_alphaskia_text_metrics_get_width(napi_env env, napi_callback_info info)
{
  napi_status status(napi_ok);

  CHECK_ARGS(1);
  GET_ARGUMENT_HANDLE(0, alphaskia_text_metrics_t, text_metrics);

  float value = alphaskia_text_metrics_get_width(text_metrics);
  napi_value node_value;
  status = napi_create_double(env, static_cast<double>(value), &node_value);
  ASSERT_STATUS();
  return node_value;
}

static napi_value node_alphaskia_text_metrics_get_actual_bounding_box_left(napi_env env, napi_callback_info info)
{
  napi_status status(napi_ok);

  CHECK_ARGS(1);
  GET_ARGUMENT_HANDLE(0, alphaskia_text_metrics_t, text_metrics);

  float value = alphaskia_text_metrics_get_actual_bounding_box_left(text_metrics);
  napi_value node_value;
  status = napi_create_double(env, static_cast<double>(value), &node_value);
  ASSERT_STATUS();
  return node_value;
}

static napi_value node_alphaskia_text_metrics_get_actual_bounding_box_right(napi_env env, napi_callback_info info)
{
  napi_status status(napi_ok);

  CHECK_ARGS(1);
  GET_ARGUMENT_HANDLE(0, alphaskia_text_metrics_t, text_metrics);

  float value = alphaskia_text_metrics_get_actual_bounding_box_right(text_metrics);
  napi_value node_value;
  status = napi_create_double(env, static_cast<double>(value), &node_value);
  ASSERT_STATUS();
  return node_value;
}

static napi_value node_alphaskia_text_metrics_get_font_bounding_box_ascent(napi_env env, napi_callback_info info)
{
  napi_status status(napi_ok);

  CHECK_ARGS(1);
  GET_ARGUMENT_HANDLE(0, alphaskia_text_metrics_t, text_metrics);

  float value = alphaskia_text_metrics_get_font_bounding_box_ascent(text_metrics);
  napi_value node_value;
  status = napi_create_double(env, static_cast<double>(value), &node_value);
  ASSERT_STATUS();
  return node_value;
}

static napi_value node_alphaskia_text_metrics_get_font_bounding_box_descent(napi_env env, napi_callback_info info)
{
  napi_status status(napi_ok);

  CHECK_ARGS(1);
  GET_ARGUMENT_HANDLE(0, alphaskia_text_metrics_t, text_metrics);

  float value = alphaskia_text_metrics_get_font_bounding_box_descent(text_metrics);
  napi_value node_value;
  status = napi_create_double(env, static_cast<double>(value), &node_value);
  ASSERT_STATUS();
  return node_value;
}

static napi_value node_alphaskia_text_metrics_get_actual_bounding_box_ascent(napi_env env, napi_callback_info info)
{
  napi_status status(napi_ok);

  CHECK_ARGS(1);
  GET_ARGUMENT_HANDLE(0, alphaskia_text_metrics_t, text_metrics);

  float value = alphaskia_text_metrics_get_actual_bounding_box_ascent(text_metrics);
  napi_value node_value;
  status = napi_create_double(env, static_cast<double>(value), &node_value);
  ASSERT_STATUS();
  return node_value;
}

static napi_value node_alphaskia_text_metrics_get_actual_bounding_box_descent(napi_env env, napi_callback_info info)
{
  napi_status status(napi_ok);

  CHECK_ARGS(1);
  GET_ARGUMENT_HANDLE(0, alphaskia_text_metrics_t, text_metrics);

  float value = alphaskia_text_metrics_get_actual_bounding_box_descent(text_metrics);
  napi_value node_value;
  status = napi_create_double(env, static_cast<double>(value), &node_value);
  ASSERT_STATUS();
  return node_value;
}

static napi_value node_alphaskia_text_metrics_get_em_height_ascent(napi_env env, napi_callback_info info)
{
  napi_status status(napi_ok);

  CHECK_ARGS(1);
  GET_ARGUMENT_HANDLE(0, alphaskia_text_metrics_t, text_metrics);

  float value = alphaskia_text_metrics_get_em_height_ascent(text_metrics);
  napi_value node_value;
  status = napi_create_double(env, static_cast<double>(value), &node_value);
  ASSERT_STATUS();
  return node_value;
}

static napi_value node_alphaskia_text_metrics_get_em_height_descent(napi_env env, napi_callback_info info)
{
  napi_status status(napi_ok);

  CHECK_ARGS(1);
  GET_ARGUMENT_HANDLE(0, alphaskia_text_metrics_t, text_metrics);

  float value = alphaskia_text_metrics_get_em_height_descent(text_metrics);
  napi_value node_value;
  status = napi_create_double(env, static_cast<double>(value), &node_value);
  ASSERT_STATUS();
  return node_value;
}

static napi_value node_alphaskia_text_metrics_get_hanging_baseline(napi_env env, napi_callback_info info)
{
  napi_status status(napi_ok);

  CHECK_ARGS(1);
  GET_ARGUMENT_HANDLE(0, alphaskia_text_metrics_t, text_metrics);

  float value = alphaskia_text_metrics_get_hanging_baseline(text_metrics);
  napi_value node_value;
  status = napi_create_double(env, static_cast<double>(value), &node_value);
  ASSERT_STATUS();
  return node_value;
}

static napi_value node_alphaskia_text_metrics_get_alphabetic_baseline(napi_env env, napi_callback_info info)
{
  napi_status status(napi_ok);

  CHECK_ARGS(1);
  GET_ARGUMENT_HANDLE(0, alphaskia_text_metrics_t, text_metrics);

  float value = alphaskia_text_metrics_get_alphabetic_baseline(text_metrics);
  napi_value node_value;
  status = napi_create_double(env, static_cast<double>(value), &node_value);
  ASSERT_STATUS();
  return node_value;
}

static napi_value node_alphaskia_text_metrics_get_ideographic_baseline(napi_env env, napi_callback_info info)
{
  napi_status status(napi_ok);

  CHECK_ARGS(1);
  GET_ARGUMENT_HANDLE(0, alphaskia_text_metrics_t, text_metrics);

  float value = alphaskia_text_metrics_get_ideographic_baseline(text_metrics);
  napi_value node_value;
  status = napi_create_double(env, static_cast<double>(value), &node_value);
  ASSERT_STATUS();
  return node_value;
}

static napi_value node_alphaskia_text_metrics_free(napi_env env, napi_callback_info info)
{
  napi_status status(napi_ok);

  CHECK_ARGS(1);
  GET_ARGUMENT_HANDLE(0, alphaskia_text_metrics_t, text_metrics);

  alphaskia_text_metrics_free(text_metrics);

  void *old;
  status = napi_remove_wrap(env, node_argv[0], &old);
  ASSERT_STATUS();

  RETURN_UNDEFINED();
}

static napi_value node_alphaskia_image_get_width(napi_env env, napi_callback_info info)
{
  napi_status status(napi_ok);

  CHECK_ARGS(1);
  GET_ARGUMENT_HANDLE(0, alphaskia_image_t, image);

  int32_t width = alphaskia_image_get_width(image);
  napi_value node_width;
  status = napi_create_int32(env, width, &node_width);
  ASSERT_STATUS();
  return node_width;
}

static napi_value node_alphaskia_image_get_height(napi_env env, napi_callback_info info)
{
  napi_status status(napi_ok);

  CHECK_ARGS(1);
  GET_ARGUMENT_HANDLE(0, alphaskia_image_t, image);

  int32_t width = alphaskia_image_get_height(image);
  napi_value node_height;
  status = napi_create_int32(env, width, &node_height);
  ASSERT_STATUS();
  return node_height;
}

static napi_value node_alphaskia_image_read_pixels(napi_env env, napi_callback_info info)
{
  napi_status status(napi_ok);

  CHECK_ARGS(1);
  GET_ARGUMENT_HANDLE(0, alphaskia_image_t, image);

  uint64_t row_bytes = alphaskia_image_get_width(image) * sizeof(int32_t);
  uint64_t total_bytes = row_bytes * alphaskia_image_get_height(image);

  void *node_data;
  napi_value node_value;
  status = napi_create_arraybuffer(env, total_bytes, &node_data, &node_value);
  ASSERT_STATUS();

  uint8_t success = alphaskia_image_read_pixels(image, static_cast<uint8_t *>(node_data), row_bytes);
  if (success == 0)
  {
    RETURN_UNDEFINED();
  }
  else
  {
    return node_value;
  }
}
static napi_value node_alphaskia_image_encode_png(napi_env env, napi_callback_info info)
{
  napi_status status(napi_ok);

  CHECK_ARGS(1);
  GET_ARGUMENT_HANDLE(0, alphaskia_image_t, image);

  alphaskia_data_t png = alphaskia_image_encode_png(image);
  uint64_t png_size = alphaskia_data_get_length(png);
  uint8_t *png_data = alphaskia_data_get_data(png);

  void *node_data;
  napi_value node_value;
  status = napi_create_arraybuffer(env, png_size, &node_data, &node_value);
  ASSERT_STATUS();
  memcpy(node_data, png_data, static_cast<size_t>(png_size));

  return node_value;
}
static napi_value node_alphaskia_image_free(napi_env env, napi_callback_info info)
{
  napi_status status(napi_ok);

  CHECK_ARGS(1);
  GET_ARGUMENT_HANDLE(0, alphaskia_image_t, image);

  alphaskia_image_free(image);
  void *old;
  status = napi_remove_wrap(env, node_argv[0], &old);
  ASSERT_STATUS();

  RETURN_UNDEFINED();
}

static napi_value node_alphaskia_canvas_new(napi_env env, napi_callback_info info)
{
  napi_status status(napi_ok);

  alphaskia_canvas_t canvas = alphaskia_canvas_new();

  WRAP_HANDLE(alphaskia_canvas_t, wrapped, canvas);
  return wrapped;
}

static napi_value node_alphaskia_canvas_free(napi_env env, napi_callback_info info)
{
  napi_status status(napi_ok);

  CHECK_ARGS(1);
  GET_ARGUMENT_HANDLE(0, alphaskia_canvas_t, canvas);

  alphaskia_canvas_free(canvas);
  void *old;
  status = napi_remove_wrap(env, node_argv[0], &old);
  ASSERT_STATUS();

  RETURN_UNDEFINED();
}

static napi_value node_alphaskia_canvas_set_color(napi_env env, napi_callback_info info)
{
  napi_status status(napi_ok);

  CHECK_ARGS(2);
  GET_ARGUMENT_HANDLE(0, alphaskia_canvas_t, canvas);
  GET_ARGUMENT_UINT32(1, color);

  alphaskia_canvas_set_color(canvas, color);

  RETURN_UNDEFINED();
}

static napi_value node_alphaskia_canvas_get_color(napi_env env, napi_callback_info info)
{
  napi_status status(napi_ok);

  CHECK_ARGS(1);
  GET_ARGUMENT_HANDLE(0, alphaskia_canvas_t, canvas);

  uint32_t color = alphaskia_canvas_get_color(canvas);

  napi_value color_node;
  status = napi_create_uint32(env, color, &color_node);
  ASSERT_STATUS();

  return color_node;
}

static napi_value node_alphaskia_canvas_set_line_width(napi_env env, napi_callback_info info)
{
  napi_status status(napi_ok);

  CHECK_ARGS(2);
  GET_ARGUMENT_HANDLE(0, alphaskia_canvas_t, canvas);

  GET_ARGUMENT_FLOAT(1, line_width);

  alphaskia_canvas_set_line_width(canvas, line_width);

  RETURN_UNDEFINED();
}
static napi_value node_alphaskia_canvas_get_line_width(napi_env env, napi_callback_info info)
{
  napi_status status(napi_ok);

  CHECK_ARGS(1);
  GET_ARGUMENT_HANDLE(0, alphaskia_canvas_t, canvas);

  float line_width = alphaskia_canvas_get_line_width(canvas);
  napi_value line_width_node;
  status = napi_create_double(env, line_width, &line_width_node);
  ASSERT_STATUS();

  return line_width_node;
}

#include <iostream>
static napi_value node_alphaskia_canvas_begin_render(napi_env env, napi_callback_info info)
{
  napi_status status(napi_ok);

  CHECK_ARGS(4);
  GET_ARGUMENT_HANDLE(0, alphaskia_canvas_t, canvas);
  GET_ARGUMENT_INT32(1, width);
  GET_ARGUMENT_INT32(2, height);
  GET_ARGUMENT_FLOAT(3, render_scale);

  alphaskia_canvas_begin_render(canvas, width, height, render_scale);

  RETURN_UNDEFINED();
}

static napi_value node_alphaskia_canvas_end_render(napi_env env, napi_callback_info info)
{
  napi_status status(napi_ok);

  CHECK_ARGS(1);
  GET_ARGUMENT_HANDLE(0, alphaskia_canvas_t, canvas);

  alphaskia_image_t image = alphaskia_canvas_end_render(canvas);
  WRAP_HANDLE(alphaskia_image_t, wrapped, image);
  return wrapped;
}

static napi_value node_alphaskia_canvas_fill_rect(napi_env env, napi_callback_info info)
{
  napi_status status(napi_ok);

  CHECK_ARGS(5);
  GET_ARGUMENT_HANDLE(0, alphaskia_canvas_t, canvas);
  GET_ARGUMENT_FLOAT(1, x);
  GET_ARGUMENT_FLOAT(2, y);
  GET_ARGUMENT_FLOAT(3, w);
  GET_ARGUMENT_FLOAT(4, h);

  alphaskia_canvas_fill_rect(canvas, x, y, w, h);

  RETURN_UNDEFINED();
}
static napi_value node_alphaskia_canvas_stroke_rect(napi_env env, napi_callback_info info)
{
  napi_status status(napi_ok);

  CHECK_ARGS(5);
  GET_ARGUMENT_HANDLE(0, alphaskia_canvas_t, canvas);
  GET_ARGUMENT_FLOAT(1, x);
  GET_ARGUMENT_FLOAT(2, y);
  GET_ARGUMENT_FLOAT(3, w);
  GET_ARGUMENT_FLOAT(4, h);

  alphaskia_canvas_stroke_rect(canvas, x, y, w, h);

  RETURN_UNDEFINED();
}

static napi_value node_alphaskia_canvas_begin_path(napi_env env, napi_callback_info info)
{
  napi_status status(napi_ok);

  CHECK_ARGS(1);
  GET_ARGUMENT_HANDLE(0, alphaskia_canvas_t, canvas);

  alphaskia_canvas_begin_path(canvas);

  RETURN_UNDEFINED();
}

static napi_value node_alphaskia_canvas_close_path(napi_env env, napi_callback_info info)
{
  napi_status status(napi_ok);

  CHECK_ARGS(1);
  GET_ARGUMENT_HANDLE(0, alphaskia_canvas_t, canvas);

  alphaskia_canvas_close_path(canvas);

  RETURN_UNDEFINED();
}

static napi_value node_alphaskia_canvas_move_to(napi_env env, napi_callback_info info)
{
  napi_status status(napi_ok);

  CHECK_ARGS(3);
  GET_ARGUMENT_HANDLE(0, alphaskia_canvas_t, canvas);
  GET_ARGUMENT_FLOAT(1, x);
  GET_ARGUMENT_FLOAT(2, y);

  alphaskia_canvas_move_to(canvas, x, y);

  RETURN_UNDEFINED();
}

static napi_value node_alphaskia_canvas_line_to(napi_env env, napi_callback_info info)
{
  napi_status status(napi_ok);

  CHECK_ARGS(3);
  GET_ARGUMENT_HANDLE(0, alphaskia_canvas_t, canvas);
  GET_ARGUMENT_FLOAT(1, x);
  GET_ARGUMENT_FLOAT(2, y);

  alphaskia_canvas_line_to(canvas, x, y);

  RETURN_UNDEFINED();
}
static napi_value node_alphaskia_canvas_quadratic_curve_to(napi_env env, napi_callback_info info)
{
  napi_status status(napi_ok);

  CHECK_ARGS(5);
  GET_ARGUMENT_HANDLE(0, alphaskia_canvas_t, canvas);
  GET_ARGUMENT_FLOAT(1, cpx);
  GET_ARGUMENT_FLOAT(2, cpy);
  GET_ARGUMENT_FLOAT(3, x);
  GET_ARGUMENT_FLOAT(4, y);

  alphaskia_canvas_quadratic_curve_to(canvas, cpx, cpy, x, y);

  RETURN_UNDEFINED();
}

static napi_value node_alphaskia_canvas_bezier_curve_to(napi_env env, napi_callback_info info)
{
  napi_status status(napi_ok);

  CHECK_ARGS(7);
  GET_ARGUMENT_HANDLE(0, alphaskia_canvas_t, canvas);
  GET_ARGUMENT_FLOAT(1, cp1x);
  GET_ARGUMENT_FLOAT(2, cp1y);
  GET_ARGUMENT_FLOAT(3, cp2x);
  GET_ARGUMENT_FLOAT(4, cp2y);
  GET_ARGUMENT_FLOAT(5, x);
  GET_ARGUMENT_FLOAT(6, y);

  alphaskia_canvas_bezier_curve_to(canvas, cp1x, cp1y, cp2x, cp2y, x, y);

  RETURN_UNDEFINED();
}

static napi_value node_alphaskia_canvas_fill_circle(napi_env env, napi_callback_info info)
{
  napi_status status(napi_ok);

  CHECK_ARGS(4);
  GET_ARGUMENT_HANDLE(0, alphaskia_canvas_t, canvas);
  GET_ARGUMENT_FLOAT(1, x);
  GET_ARGUMENT_FLOAT(2, y);
  GET_ARGUMENT_FLOAT(3, radius);

  alphaskia_canvas_fill_circle(canvas, x, y, radius);

  RETURN_UNDEFINED();
}

static napi_value node_alphaskia_canvas_stroke_circle(napi_env env, napi_callback_info info)
{
  napi_status status(napi_ok);

  CHECK_ARGS(4);
  GET_ARGUMENT_HANDLE(0, alphaskia_canvas_t, canvas);
  GET_ARGUMENT_FLOAT(1, x);
  GET_ARGUMENT_FLOAT(2, y);
  GET_ARGUMENT_FLOAT(3, radius);

  alphaskia_canvas_stroke_circle(canvas, x, y, radius);

  RETURN_UNDEFINED();
}

static napi_value node_alphaskia_canvas_fill(napi_env env, napi_callback_info info)
{
  napi_status status(napi_ok);

  CHECK_ARGS(1);
  GET_ARGUMENT_HANDLE(0, alphaskia_canvas_t, canvas);

  alphaskia_canvas_fill(canvas);

  RETURN_UNDEFINED();
}

static napi_value node_alphaskia_canvas_stroke(napi_env env, napi_callback_info info)
{
  napi_status status(napi_ok);

  CHECK_ARGS(1);
  GET_ARGUMENT_HANDLE(0, alphaskia_canvas_t, canvas);

  alphaskia_canvas_stroke(canvas);

  RETURN_UNDEFINED();
}

static napi_value node_alphaskia_canvas_draw_image(napi_env env, napi_callback_info info)
{
  napi_status status(napi_ok);

  CHECK_ARGS(6);
  GET_ARGUMENT_HANDLE(0, alphaskia_canvas_t, canvas);
  GET_ARGUMENT_HANDLE(1, alphaskia_image_t, image);
  GET_ARGUMENT_FLOAT(2, x);
  GET_ARGUMENT_FLOAT(3, y);
  GET_ARGUMENT_FLOAT(4, w);
  GET_ARGUMENT_FLOAT(5, h);

  alphaskia_canvas_draw_image(canvas, image, x, y, w, h);

  RETURN_UNDEFINED();
}

static napi_value node_alphaskia_image_from_pixels(napi_env env, napi_callback_info info)
{
  napi_status status(napi_ok);

  CHECK_ARGS(3);
  GET_ARGUMENT_INT32(0, width);
  GET_ARGUMENT_INT32(1, height);

  CHECK_ARGUMENT_IS_ARRAYBUFFER(2);

  void *node_pixels(nullptr);
  size_t node_pixels_length(0);
  status = napi_get_arraybuffer_info(env, node_argv[2], &node_pixels, &node_pixels_length);
  ASSERT_STATUS();

  alphaskia_image_t image = alphaskia_image_from_pixels(width, height, reinterpret_cast<const uint8_t *>(node_pixels));
  WRAP_HANDLE(alphaskia_image_t, wrapped, image);
  return wrapped;
}

static napi_value node_alphaskia_image_decode(napi_env env, napi_callback_info info)
{
  napi_status status(napi_ok);

  CHECK_ARGS(1);
  CHECK_ARGUMENT_IS_ARRAYBUFFER(0);

  void *node_bytes(nullptr);
  size_t node_bytes_length(0);
  status = napi_get_arraybuffer_info(env, node_argv[0], &node_bytes, &node_bytes_length);
  ASSERT_STATUS();

  alphaskia_image_t image = alphaskia_image_decode(reinterpret_cast<const uint8_t *>(node_bytes), node_bytes_length);
  WRAP_HANDLE(alphaskia_image_t, wrapped, image);
  return wrapped;
}

static napi_value node_alphaskia_canvas_fill_text(napi_env env, napi_callback_info info)
{
  napi_status status(napi_ok);

  CHECK_ARGS(8);
  GET_ARGUMENT_HANDLE(0, alphaskia_canvas_t, canvas);
  GET_ARGUMENT_UTF16_STRING(1, text);
  GET_ARGUMENT_HANDLE(2, alphaskia_text_style_t, text_style);
  GET_ARGUMENT_FLOAT(3, font_size);
  GET_ARGUMENT_FLOAT(4, x);
  GET_ARGUMENT_FLOAT(5, y);
  GET_ARGUMENT_INT32(6, text_align);
  GET_ARGUMENT_INT32(7, baseline);

  alphaskia_canvas_fill_text(canvas, text.c_str(), text.length(), text_style, font_size, x, y, static_cast<alphaskia_text_align_t>(text_align), static_cast<alphaskia_text_baseline_t>(baseline));

  RETURN_UNDEFINED();
}

static napi_value node_alphaskia_canvas_measure_text(napi_env env, napi_callback_info info)
{
  napi_status status(napi_ok);

  CHECK_ARGS(6);
  GET_ARGUMENT_HANDLE(0, alphaskia_canvas_t, canvas);
  GET_ARGUMENT_UTF16_STRING(1, text);
  GET_ARGUMENT_HANDLE(2, alphaskia_text_style_t, text_style);
  GET_ARGUMENT_FLOAT(3, font_size);
  GET_ARGUMENT_INT32(4, text_align);
  GET_ARGUMENT_INT32(5, baseline);

  auto text_metrics = alphaskia_canvas_measure_text(canvas, text.c_str(), text.length(), text_style, font_size, static_cast<alphaskia_text_align_t>(text_align), static_cast<alphaskia_text_baseline_t>(baseline));
  WRAP_HANDLE(alphaskia_text_metrics_t, wrapped, text_metrics);
  return wrapped;
}

static napi_value node_alphaskia_canvas_begin_rotate(napi_env env, napi_callback_info info)
{
  napi_status status(napi_ok);

  CHECK_ARGS(4);
  GET_ARGUMENT_HANDLE(0, alphaskia_canvas_t, canvas);
  GET_ARGUMENT_FLOAT(1, center_x);
  GET_ARGUMENT_FLOAT(2, center_y);
  GET_ARGUMENT_FLOAT(3, angle);

  alphaskia_canvas_begin_rotate(canvas, center_x, center_y, angle);

  RETURN_UNDEFINED();
}
static napi_value node_alphaskia_canvas_end_rotate(napi_env env, napi_callback_info info)
{
  napi_status status(napi_ok);

  CHECK_ARGS(1);
  GET_ARGUMENT_HANDLE(0, alphaskia_canvas_t, canvas);

  alphaskia_canvas_end_rotate(canvas);

  RETURN_UNDEFINED();
}

#define EXPORT_NODE_FUNCTION(name) methods.push_back({/* utf8name: */ STRINGIFY(name),   \
                                                      /* name: */ nullptr,               \
                                                      /* method: */ CONCAT(node_, name), \
                                                      /* getter: */ nullptr,             \
                                                      /* setter: */ nullptr,             \
                                                      /* value: */ nullptr,              \
                                                      /* attributes: */ napi_default,    \
                                                      /* data: */ nullptr});

napi_value init(napi_env env, napi_value exports)
{
  std::vector<napi_property_descriptor> methods;

  EXPORT_NODE_FUNCTION(alphaskia_get_color_type);
  EXPORT_NODE_FUNCTION(alphaskia_switch_to_freetype_fonts);
  EXPORT_NODE_FUNCTION(alphaskia_switch_to_operating_system_fonts);

  EXPORT_NODE_FUNCTION(alphaskia_data_new_copy);
  EXPORT_NODE_FUNCTION(alphaskia_data_get_data);
  // EXPORT_NODE_FUNCTION(alphaskia_data_get_length);
  EXPORT_NODE_FUNCTION(alphaskia_data_free);

  EXPORT_NODE_FUNCTION(alphaskia_typeface_register);
  EXPORT_NODE_FUNCTION(alphaskia_typeface_free);
  EXPORT_NODE_FUNCTION(alphaskia_typeface_make_from_name);
  EXPORT_NODE_FUNCTION(alphaskia_typeface_get_family_name);
  EXPORT_NODE_FUNCTION(alphaskia_typeface_get_weight);
  EXPORT_NODE_FUNCTION(alphaskia_typeface_is_italic);

  EXPORT_NODE_FUNCTION(alphaskia_text_style_new);
  // EXPORT_NODE_FUNCTION(alphaskia_text_style_get_family_name_count);
  // EXPORT_NODE_FUNCTION(alphaskia_text_style_get_family_name);
  // EXPORT_NODE_FUNCTION(alphaskia_text_style_get_weight);
  // EXPORT_NODE_FUNCTION(alphaskia_text_style_is_italic);
  EXPORT_NODE_FUNCTION(alphaskia_text_style_free);

  EXPORT_NODE_FUNCTION(alphaskia_text_metrics_get_width);
  EXPORT_NODE_FUNCTION(alphaskia_text_metrics_get_actual_bounding_box_left);
  EXPORT_NODE_FUNCTION(alphaskia_text_metrics_get_actual_bounding_box_right);
  EXPORT_NODE_FUNCTION(alphaskia_text_metrics_get_font_bounding_box_ascent);
  EXPORT_NODE_FUNCTION(alphaskia_text_metrics_get_font_bounding_box_descent);
  EXPORT_NODE_FUNCTION(alphaskia_text_metrics_get_actual_bounding_box_ascent);
  EXPORT_NODE_FUNCTION(alphaskia_text_metrics_get_actual_bounding_box_descent);
  EXPORT_NODE_FUNCTION(alphaskia_text_metrics_get_em_height_ascent);
  EXPORT_NODE_FUNCTION(alphaskia_text_metrics_get_em_height_descent);
  EXPORT_NODE_FUNCTION(alphaskia_text_metrics_get_hanging_baseline);
  EXPORT_NODE_FUNCTION(alphaskia_text_metrics_get_alphabetic_baseline);
  EXPORT_NODE_FUNCTION(alphaskia_text_metrics_get_ideographic_baseline);
  EXPORT_NODE_FUNCTION(alphaskia_text_metrics_free);

  EXPORT_NODE_FUNCTION(alphaskia_image_get_width);
  EXPORT_NODE_FUNCTION(alphaskia_image_get_height);
  EXPORT_NODE_FUNCTION(alphaskia_image_read_pixels);
  EXPORT_NODE_FUNCTION(alphaskia_image_encode_png);
  EXPORT_NODE_FUNCTION(alphaskia_image_from_pixels);
  EXPORT_NODE_FUNCTION(alphaskia_image_decode);
  EXPORT_NODE_FUNCTION(alphaskia_image_free);

  EXPORT_NODE_FUNCTION(alphaskia_canvas_new);
  EXPORT_NODE_FUNCTION(alphaskia_canvas_free);

  EXPORT_NODE_FUNCTION(alphaskia_canvas_set_color);
  EXPORT_NODE_FUNCTION(alphaskia_canvas_get_color);

  EXPORT_NODE_FUNCTION(alphaskia_canvas_set_line_width);
  EXPORT_NODE_FUNCTION(alphaskia_canvas_get_line_width);

  EXPORT_NODE_FUNCTION(alphaskia_canvas_begin_render);
  EXPORT_NODE_FUNCTION(alphaskia_canvas_end_render);
  EXPORT_NODE_FUNCTION(alphaskia_canvas_fill_rect);
  EXPORT_NODE_FUNCTION(alphaskia_canvas_stroke_rect);
  EXPORT_NODE_FUNCTION(alphaskia_canvas_begin_path);
  EXPORT_NODE_FUNCTION(alphaskia_canvas_close_path);
  EXPORT_NODE_FUNCTION(alphaskia_canvas_move_to);
  EXPORT_NODE_FUNCTION(alphaskia_canvas_line_to);
  EXPORT_NODE_FUNCTION(alphaskia_canvas_quadratic_curve_to);
  EXPORT_NODE_FUNCTION(alphaskia_canvas_bezier_curve_to);
  EXPORT_NODE_FUNCTION(alphaskia_canvas_fill_circle);
  EXPORT_NODE_FUNCTION(alphaskia_canvas_stroke_circle);
  EXPORT_NODE_FUNCTION(alphaskia_canvas_fill);
  EXPORT_NODE_FUNCTION(alphaskia_canvas_stroke);
  EXPORT_NODE_FUNCTION(alphaskia_canvas_draw_image);

  EXPORT_NODE_FUNCTION(alphaskia_canvas_fill_text);

  EXPORT_NODE_FUNCTION(alphaskia_canvas_measure_text);
  EXPORT_NODE_FUNCTION(alphaskia_canvas_begin_rotate);
  EXPORT_NODE_FUNCTION(alphaskia_canvas_end_rotate);

  napi_status status = napi_define_properties(env, exports, methods.size(), methods.data());
  ASSERT_STATUS();
  return exports;
}

NAPI_MODULE("alphaskia", init)