#include "../include/alphaTab_alphaSkia_AlphaSkiaTextMetrics.h"
#include "../include/JniHelper.h"
#include "../../../../wrapper/include/alphaskia.h"

extern "C"
{
    JNIEXPORT jfloat JNICALL Java_alphaTab_alphaSkia_AlphaSkiaTextMetrics_getWidth(JNIEnv *env, jobject instance)
    {
        jlong handle = get_handle(env, instance);
        CHECK_HANDLE_RETURN(handle, static_cast<jfloat>(0))

        uint16_t value = alphaskia_text_metrics_get_width(reinterpret_cast<alphaskia_text_metrics_t>(handle));
        return static_cast<jint>(value);
    }

    JNIEXPORT jfloat JNICALL Java_alphaTab_alphaSkia_AlphaSkiaTextMetrics_getActualBoundingBoxLeft(JNIEnv *env, jobject instance)
    {
        jlong handle = get_handle(env, instance);
        CHECK_HANDLE_RETURN(handle, static_cast<jfloat>(0))

        uint16_t value = alphaskia_text_metrics_get_actual_bounding_box_left(reinterpret_cast<alphaskia_text_metrics_t>(handle));
        return static_cast<jfloat>(value);
    }

    JNIEXPORT jfloat JNICALL Java_alphaTab_alphaSkia_AlphaSkiaTextMetrics_getActualBoundingBoxRight(JNIEnv *env, jobject instance)
    {
        jlong handle = get_handle(env, instance);
        CHECK_HANDLE_RETURN(handle, static_cast<jfloat>(0))

        uint16_t value = alphaskia_text_metrics_get_actual_bounding_box_right(reinterpret_cast<alphaskia_text_metrics_t>(handle));
        return static_cast<jfloat>(value);
    }

    JNIEXPORT jfloat JNICALL Java_alphaTab_alphaSkia_AlphaSkiaTextMetrics_getFontBoundingBoxAscent(JNIEnv *env, jobject instance)
    {
        jlong handle = get_handle(env, instance);
        CHECK_HANDLE_RETURN(handle, static_cast<jfloat>(0))

        uint16_t value = alphaskia_text_metrics_get_font_bounding_box_ascent(reinterpret_cast<alphaskia_text_metrics_t>(handle));
        return static_cast<jfloat>(value);
    }

    JNIEXPORT jfloat JNICALL Java_alphaTab_alphaSkia_AlphaSkiaTextMetrics_getFontBoundingBoxDescent(JNIEnv *env, jobject instance)
    {
        jlong handle = get_handle(env, instance);
        CHECK_HANDLE_RETURN(handle, static_cast<jfloat>(0))

        uint16_t value = alphaskia_text_metrics_get_font_bounding_box_descent(reinterpret_cast<alphaskia_text_metrics_t>(handle));
        return static_cast<jfloat>(value);
    }

    JNIEXPORT jfloat JNICALL Java_alphaTab_alphaSkia_AlphaSkiaTextMetrics_getActualBoundingBoxAscent(JNIEnv *env, jobject instance)
    {
        jlong handle = get_handle(env, instance);
        CHECK_HANDLE_RETURN(handle, static_cast<jfloat>(0))

        uint16_t value = alphaskia_text_metrics_get_actual_bounding_box_ascent(reinterpret_cast<alphaskia_text_metrics_t>(handle));
        return static_cast<jfloat>(value);
    }

    JNIEXPORT jfloat JNICALL Java_alphaTab_alphaSkia_AlphaSkiaTextMetrics_getActualBoundingBoxDescent(JNIEnv *env, jobject instance)
    {
        jlong handle = get_handle(env, instance);
        CHECK_HANDLE_RETURN(handle, static_cast<jfloat>(0))

        uint16_t value = alphaskia_text_metrics_get_actual_bounding_box_descent(reinterpret_cast<alphaskia_text_metrics_t>(handle));
        return static_cast<jfloat>(value);
    }

    JNIEXPORT jfloat JNICALL Java_alphaTab_alphaSkia_AlphaSkiaTextMetrics_getEmHeightAscent(JNIEnv *env, jobject instance)
    {
        jlong handle = get_handle(env, instance);
        CHECK_HANDLE_RETURN(handle, static_cast<jfloat>(0))

        uint16_t value = alphaskia_text_metrics_get_em_height_ascent(reinterpret_cast<alphaskia_text_metrics_t>(handle));
        return static_cast<jfloat>(value);
    }

    JNIEXPORT jfloat JNICALL Java_alphaTab_alphaSkia_AlphaSkiaTextMetrics_getEmHeightDescent(JNIEnv *env, jobject instance)
    {
        jlong handle = get_handle(env, instance);
        CHECK_HANDLE_RETURN(handle, static_cast<jfloat>(0))

        uint16_t value = alphaskia_text_metrics_get_em_height_descent(reinterpret_cast<alphaskia_text_metrics_t>(handle));
        return static_cast<jfloat>(value);
    }

    JNIEXPORT jfloat JNICALL Java_alphaTab_alphaSkia_AlphaSkiaTextMetrics_getHangingBaseline(JNIEnv *env, jobject instance)
    {
        jlong handle = get_handle(env, instance);
        CHECK_HANDLE_RETURN(handle, static_cast<jfloat>(0))

        uint16_t value = alphaskia_text_metrics_get_hanging_baseline(reinterpret_cast<alphaskia_text_metrics_t>(handle));
        return static_cast<jfloat>(value);
    }

    JNIEXPORT jfloat JNICALL Java_alphaTab_alphaSkia_AlphaSkiaTextMetrics_getAlphabeticBaseline(JNIEnv *env, jobject instance)
    {
        jlong handle = get_handle(env, instance);
        CHECK_HANDLE_RETURN(handle, static_cast<jfloat>(0))

        uint16_t value = alphaskia_text_metrics_get_alphabetic_baseline(reinterpret_cast<alphaskia_text_metrics_t>(handle));
        return static_cast<jfloat>(value);
    }

    JNIEXPORT jfloat JNICALL Java_alphaTab_alphaSkia_AlphaSkiaTextMetrics_getIdeographicBaseline(JNIEnv *env, jobject instance)
    {
        jlong handle = get_handle(env, instance);
        CHECK_HANDLE_RETURN(handle, static_cast<jfloat>(0))

        uint16_t value = alphaskia_text_metrics_get_ideographic_baseline(reinterpret_cast<alphaskia_text_metrics_t>(handle));
        return static_cast<jfloat>(value);
    }

    JNIEXPORT void JNICALL Java_alphaTab_alphaSkia_AlphaSkiaTextMetrics_close(JNIEnv *env, jobject instance)
    {
        jlong handle = get_handle(env, instance);
        CHECK_HANDLE(handle)

        alphaskia_text_metrics_t data = reinterpret_cast<alphaskia_text_metrics_t>(handle);
        alphaskia_text_metrics_free(data);
        set_handle(env, instance, 0);
    }
}