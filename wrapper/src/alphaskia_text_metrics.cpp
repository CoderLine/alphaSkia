#include "../include/alphaskia.h"
#include "../include/AlphaSkiaCanvas.h"

extern "C"
{
    AS_API float alphaskia_text_metrics_get_width(alphaskia_text_metrics_t text_metrics)
    {
        return reinterpret_cast<AlphaSkiaTextMetrics *>(text_metrics)->get_width();
    }

    AS_API float alphaskia_text_metrics_get_actual_bounding_box_left(alphaskia_text_metrics_t text_metrics)
    {
        return reinterpret_cast<AlphaSkiaTextMetrics *>(text_metrics)->get_actual_bounding_box_left();
    }

    AS_API float alphaskia_text_metrics_get_actual_bounding_box_right(alphaskia_text_metrics_t text_metrics)
    {
        return reinterpret_cast<AlphaSkiaTextMetrics *>(text_metrics)->get_actual_bounding_box_right();
    }

    AS_API float alphaskia_text_metrics_get_font_bounding_box_ascent(alphaskia_text_metrics_t text_metrics)
    {
        return reinterpret_cast<AlphaSkiaTextMetrics *>(text_metrics)->get_font_bounding_box_ascent();
    }

    AS_API float alphaskia_text_metrics_get_font_bounding_box_descent(alphaskia_text_metrics_t text_metrics)
    {
        return reinterpret_cast<AlphaSkiaTextMetrics *>(text_metrics)->get_font_bounding_box_descent();
    }

    AS_API float alphaskia_text_metrics_get_actual_bounding_box_ascent(alphaskia_text_metrics_t text_metrics)
    {
        return reinterpret_cast<AlphaSkiaTextMetrics *>(text_metrics)->get_actual_bounding_box_ascent();
    }

    AS_API float alphaskia_text_metrics_get_actual_bounding_box_descent(alphaskia_text_metrics_t text_metrics)
    {
        return reinterpret_cast<AlphaSkiaTextMetrics *>(text_metrics)->get_actual_bounding_box_descent();
    }

    AS_API float alphaskia_text_metrics_get_em_height_ascent(alphaskia_text_metrics_t text_metrics)
    {
        return reinterpret_cast<AlphaSkiaTextMetrics *>(text_metrics)->get_em_height_ascent();
    }

    AS_API float alphaskia_text_metrics_get_em_height_descent(alphaskia_text_metrics_t text_metrics)
    {
        return reinterpret_cast<AlphaSkiaTextMetrics *>(text_metrics)->get_em_height_descent();
    }

    AS_API float alphaskia_text_metrics_get_hanging_baseline(alphaskia_text_metrics_t text_metrics)
    {
        return reinterpret_cast<AlphaSkiaTextMetrics *>(text_metrics)->get_hanging_baseline();
    }

    AS_API float alphaskia_text_metrics_get_alphabetic_baseline(alphaskia_text_metrics_t text_metrics)
    {
        return reinterpret_cast<AlphaSkiaTextMetrics *>(text_metrics)->get_alphabetic_baseline();
    }

    AS_API float alphaskia_text_metrics_get_ideographic_baseline(alphaskia_text_metrics_t text_metrics)
    {
        return reinterpret_cast<AlphaSkiaTextMetrics *>(text_metrics)->get_ideographic_baseline();
    }

    AS_API void alphaskia_text_metrics_free(alphaskia_text_metrics_t text_metrics)
    {
        auto alphaSkiaTextMetrics = reinterpret_cast<AlphaSkiaTextMetrics *>(text_metrics);
        delete alphaSkiaTextMetrics;
    }
}