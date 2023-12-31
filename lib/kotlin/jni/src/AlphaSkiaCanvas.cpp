#include "../include/alphaTab_alphaSkia_NativeMethods.h"
#include "../../../../wrapper/include/alphaskia.h"

extern "C"
{
    JNIEXPORT jint JNICALL Java_alphaTab_alphaSkia_NativeMethods_alphaskiaGetColorType(JNIEnv * env, jclass clz)
    {
        return static_cast<jint>(alphaskia_get_color_type());
    }

    JNIEXPORT void JNICALL Java_alphaTab_alphaSkia_NativeMethods_alphaskiaCanvasSetColor(JNIEnv * env, jclass clz, jlong handle, jint color)
    {
        alphaskia_canvas_t canvas = reinterpret_cast<alphaskia_canvas_t>(handle);
        alphaskia_canvas_set_color(canvas, static_cast<uint32_t>(color));
    }

    JNIEXPORT jint JNICALL Java_alphaTab_alphaSkia_NativeMethods_alphaskiaCanvasGetColor(JNIEnv * env, jclass clz, jlong handle)
    {
        alphaskia_canvas_t canvas = reinterpret_cast<alphaskia_canvas_t>(handle);
        return static_cast<jint>(alphaskia_canvas_get_color(canvas));
    }

    JNIEXPORT jfloat JNICALL Java_alphaTab_alphaSkia_NativeMethods_alphaskiaCanvasGetLineWidth(JNIEnv * env, jclass clz, jlong handle)
    {
        alphaskia_canvas_t canvas = reinterpret_cast<alphaskia_canvas_t>(handle);
        return static_cast<jfloat>(alphaskia_canvas_get_line_width(canvas));
    }

    JNIEXPORT void JNICALL Java_alphaTab_alphaSkia_NativeMethods_alphaskiaCanvasSetLineWidth(JNIEnv * env, jclass clz, jlong handle, jfloat line_width)
    {
        alphaskia_canvas_t canvas = reinterpret_cast<alphaskia_canvas_t>(handle);
        alphaskia_canvas_set_line_width(canvas, static_cast<float>(line_width));
    }

    JNIEXPORT void JNICALL Java_alphaTab_alphaSkia_NativeMethods_alphaskiaCanvasFree(JNIEnv * env, jclass clz, jlong handle)
    {
        alphaskia_canvas_t canvas = reinterpret_cast<alphaskia_canvas_t>(handle);
        alphaskia_canvas_free(canvas);
    }

    JNIEXPORT void JNICALL Java_alphaTab_alphaSkia_NativeMethods_alphaskiaCanvasBeginRender(JNIEnv * env, jclass clz, jlong handle, jint width, jint height, jfloat renderScale)
    {
        alphaskia_canvas_t canvas = reinterpret_cast<alphaskia_canvas_t>(handle);
        alphaskia_canvas_begin_render(canvas, width, height, static_cast<float>(renderScale));
    }

    JNIEXPORT void JNICALL Java_alphaTab_alphaSkia_NativeMethods_alphaskiaCanvasDrawImage(JNIEnv * env, jclass clz, jlong handle, jlong image, jfloat x, jfloat y, jfloat w, jfloat h)
    {
        alphaskia_canvas_t canvas = reinterpret_cast<alphaskia_canvas_t>(handle);
        alphaskia_image_t nativeImage = reinterpret_cast<alphaskia_image_t>(image);
        alphaskia_canvas_draw_image(canvas, nativeImage, static_cast<float>(x), static_cast<float>(y), static_cast<float>(w), static_cast<float>(h));
    }

    JNIEXPORT jlong JNICALL Java_alphaTab_alphaSkia_NativeMethods_alphaskiaCanvasEndRender(JNIEnv * env, jclass clz, jlong handle)
    {
        alphaskia_canvas_t canvas = reinterpret_cast<alphaskia_canvas_t>(handle);
        alphaskia_image_t image = alphaskia_canvas_end_render(canvas);
        return static_cast<jlong>(reinterpret_cast<std::uintptr_t>(image));
    }

    JNIEXPORT void JNICALL Java_alphaTab_alphaSkia_NativeMethods_alphaskiaCanvasFillRect(JNIEnv * env, jclass clz, jlong handle, jfloat x, jfloat y, jfloat w, jfloat h)
    {
        alphaskia_canvas_t canvas = reinterpret_cast<alphaskia_canvas_t>(handle);
        alphaskia_canvas_fill_rect(canvas, x, y, w, h);
    }

    JNIEXPORT void JNICALL Java_alphaTab_alphaSkia_NativeMethods_alphaskiaCanvasStrokeRect(JNIEnv * env, jclass clz, jlong handle, jfloat x, jfloat y, jfloat w, jfloat h)
    {
        alphaskia_canvas_t canvas = reinterpret_cast<alphaskia_canvas_t>(handle);
        alphaskia_canvas_stroke_rect(canvas, x, y, w, h);
    }

    JNIEXPORT void JNICALL Java_alphaTab_alphaSkia_NativeMethods_alphaskiaCanvasBeginPath(JNIEnv * env, jclass clz, jlong handle)
    {
        alphaskia_canvas_t canvas = reinterpret_cast<alphaskia_canvas_t>(handle);
        alphaskia_canvas_begin_path(canvas);
    }

    JNIEXPORT void JNICALL Java_alphaTab_alphaSkia_NativeMethods_alphaskiaCanvasClosePath(JNIEnv * env, jclass clz, jlong handle)
    {
        alphaskia_canvas_t canvas = reinterpret_cast<alphaskia_canvas_t>(handle);
        alphaskia_canvas_close_path(canvas);
    }

    JNIEXPORT void JNICALL Java_alphaTab_alphaSkia_NativeMethods_alphaskiaCanvasMoveTo(JNIEnv * env, jclass clz, jlong handle, jfloat x, jfloat y)
    {
        alphaskia_canvas_t canvas = reinterpret_cast<alphaskia_canvas_t>(handle);
        alphaskia_canvas_move_to(canvas, x, y);
    }

    JNIEXPORT void JNICALL Java_alphaTab_alphaSkia_NativeMethods_alphaskiaCanvasLineTo(JNIEnv * env, jclass clz, jlong handle, jfloat x, jfloat y)
    {
        alphaskia_canvas_t canvas = reinterpret_cast<alphaskia_canvas_t>(handle);
        alphaskia_canvas_line_to(canvas, x, y);
    }

    JNIEXPORT void JNICALL Java_alphaTab_alphaSkia_NativeMethods_alphaskiaCanvasQuadraticCurveTo(JNIEnv * env, jclass clz, jlong handle, jfloat cpx, jfloat cpy, jfloat x, jfloat y)
    {
        alphaskia_canvas_t canvas = reinterpret_cast<alphaskia_canvas_t>(handle);
        alphaskia_canvas_quadratic_curve_to(canvas, cpx, cpy, x, y);
    }

    JNIEXPORT void JNICALL Java_alphaTab_alphaSkia_NativeMethods_alphaskiaCanvasBezierCurveTo(JNIEnv * env, jclass clz, jlong handle, jfloat cp1x, jfloat cp1y, jfloat cp2x, jfloat cp2y, jfloat x, jfloat y)
    {
        alphaskia_canvas_t canvas = reinterpret_cast<alphaskia_canvas_t>(handle);
        alphaskia_canvas_bezier_curve_to(canvas, cp1x, cp1y, cp2x, cp2y, x, y);
    }

    JNIEXPORT void JNICALL Java_alphaTab_alphaSkia_NativeMethods_alphaskiaCanvasFillCircle(JNIEnv * env, jclass clz, jlong handle, jfloat x, jfloat y, jfloat r)
    {
        alphaskia_canvas_t canvas = reinterpret_cast<alphaskia_canvas_t>(handle);
        alphaskia_canvas_fill_circle(canvas, x, y, r);
    }

    JNIEXPORT void JNICALL Java_alphaTab_alphaSkia_NativeMethods_alphaskiaCanvasStrokeCircle(JNIEnv * env, jclass clz, jlong handle, jfloat x, jfloat y, jfloat r)
    {
        alphaskia_canvas_t canvas = reinterpret_cast<alphaskia_canvas_t>(handle);
        alphaskia_canvas_stroke_circle(canvas, x, y, r);
    }

    JNIEXPORT void JNICALL Java_alphaTab_alphaSkia_NativeMethods_alphaskiaCanvasFill(JNIEnv * env, jclass clz, jlong handle)
    {
        alphaskia_canvas_t canvas = reinterpret_cast<alphaskia_canvas_t>(handle);
        alphaskia_canvas_fill(canvas);
    }

    JNIEXPORT void JNICALL Java_alphaTab_alphaSkia_NativeMethods_alphaskiaCanvasStroke(JNIEnv * env, jclass clz, jlong handle)
    {
        alphaskia_canvas_t canvas = reinterpret_cast<alphaskia_canvas_t>(handle);
        alphaskia_canvas_stroke(canvas);
    }

    JNIEXPORT void JNICALL Java_alphaTab_alphaSkia_NativeMethods_alphaskiaCanvasFillText(JNIEnv * env, jclass clz, jlong handle, jstring str, jlong typeface, jfloat font_size, jfloat x, jfloat y, jint text_align, jint baseline)
    {
        alphaskia_canvas_t canvas = reinterpret_cast<alphaskia_canvas_t>(handle);

        const jchar *nativeStr = env->GetStringChars(str, nullptr);

        alphaskia_typeface_t nativeTypeface = reinterpret_cast<alphaskia_typeface_t>(typeface);

        alphaskia_text_align_t nativeTextAlign = static_cast<alphaskia_text_align_t>(text_align);
        alphaskia_text_baseline_t nativeBaseline = static_cast<alphaskia_text_baseline_t>(baseline);

        alphaskia_canvas_fill_text(canvas, reinterpret_cast<const char16_t *>(nativeStr), static_cast<int>(env->GetStringLength(str)),
                                   nativeTypeface, static_cast<float>(font_size), static_cast<float>(x), static_cast<float>(y),
                                   nativeTextAlign, nativeBaseline);

        env->ReleaseStringChars(str, nativeStr);
    }

    JNIEXPORT jfloat JNICALL Java_alphaTab_alphaSkia_NativeMethods_alphaskiaCanvasMeasureText(JNIEnv * env, jclass clz, jlong handle, jstring str, jlong typeface, jfloat font_size)
    {
        alphaskia_canvas_t canvas = reinterpret_cast<alphaskia_canvas_t>(handle);

        const jchar *nativeStr = env->GetStringChars(str, nullptr);

        alphaskia_typeface_t nativeTypeface = reinterpret_cast<alphaskia_typeface_t>(typeface);

        float width = alphaskia_canvas_measure_text(canvas, reinterpret_cast<const char16_t *>(nativeStr), static_cast<int>(env->GetStringLength(str)), nativeTypeface, static_cast<float>(font_size));

        env->ReleaseStringChars(str, nativeStr);

        return static_cast<jfloat>(width);
    }

    JNIEXPORT void JNICALL Java_alphaTab_alphaSkia_NativeMethods_alphaskiaCanvasBeginRotate(JNIEnv * env, jclass clz, jlong handle, jfloat x, jfloat y, jfloat angle)
    {
        alphaskia_canvas_t canvas = reinterpret_cast<alphaskia_canvas_t>(handle);
        alphaskia_canvas_begin_rotate(canvas, static_cast<float>(x), static_cast<float>(y), static_cast<float>(angle));
    }

    JNIEXPORT void JNICALL Java_alphaTab_alphaSkia_NativeMethods_alphaskiaCanvasEndRotate(JNIEnv * env, jclass clz, jlong handle)
    {
        alphaskia_canvas_t canvas = reinterpret_cast<alphaskia_canvas_t>(handle);
        alphaskia_canvas_end_rotate(canvas);
    }

    JNIEXPORT jlong JNICALL Java_alphaTab_alphaSkia_NativeMethods_alphaskiaCanvasNew(JNIEnv * env, jclass clz)
    {
        alphaskia_canvas_t canvas = alphaskia_canvas_new();
        return static_cast<jlong>(reinterpret_cast<std::uintptr_t>(canvas));
    }
}