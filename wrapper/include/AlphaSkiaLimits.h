#pragma once

#include "alphaskia.h"

// Whether an image or surface of the given size is within alphaskia_limit_max_image_dimension and
// alphaskia_limit_max_image_pixels.
bool alphaskia_is_within_image_limits(int64_t width, int64_t height);

// Whether text of the given length and font size is within alphaskia_limit_max_text_length and
// alphaskia_limit_max_font_size.
bool alphaskia_is_within_text_limits(int text_length, float font_size);
