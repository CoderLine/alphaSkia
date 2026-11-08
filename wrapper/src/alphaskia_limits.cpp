#include "../include/AlphaSkiaLimits.h"

#include <atomic>

namespace
{
    constexpr int limit_count = alphaskia_limit_image_formats + 1;

    std::atomic<uint64_t> limits[limit_count] = {
        {32767},
        {uint64_t(1) << 28},
        {uint64_t(50) * 1024 * 1024},
        {4096},
        {0},
        {alphaskia_image_format_png},
    };

    uint64_t get(alphaskia_limit_t limit)
    {
        return limits[limit].load(std::memory_order_relaxed);
    }

    bool exceeds(alphaskia_limit_t limit, uint64_t value)
    {
        const uint64_t maximum = get(limit);
        return maximum != 0 && value > maximum;
    }
}

extern "C"
{
    AS_API uint64_t alphaskia_get_limit(alphaskia_limit_t limit)
    {
        if (static_cast<int>(limit) < 0 || static_cast<int>(limit) >= limit_count)
        {
            return 0;
        }
        return get(limit);
    }

    AS_API void alphaskia_set_limit(alphaskia_limit_t limit, uint64_t value)
    {
        if (static_cast<int>(limit) < 0 || static_cast<int>(limit) >= limit_count)
        {
            return;
        }
        limits[limit].store(value, std::memory_order_relaxed);
    }
}

bool alphaskia_is_within_image_limits(int64_t width, int64_t height)
{
    if (width < 0 || height < 0)
    {
        return false;
    }

    const uint64_t w = static_cast<uint64_t>(width);
    const uint64_t h = static_cast<uint64_t>(height);
    if (exceeds(alphaskia_limit_max_image_dimension, w) || exceeds(alphaskia_limit_max_image_dimension, h))
    {
        return false;
    }

    if (h != 0 && w > UINT64_MAX / h)
    {
        return false;
    }
    return !exceeds(alphaskia_limit_max_image_pixels, w * h);
}

bool alphaskia_is_within_text_limits(int text_length, float font_size)
{
    if (exceeds(alphaskia_limit_max_text_length, static_cast<uint64_t>(text_length < 0 ? 0 : text_length)))
    {
        return false;
    }

    const uint64_t max_font_size = get(alphaskia_limit_max_font_size);
    return max_font_size == 0 || font_size <= static_cast<float>(max_font_size);
}
