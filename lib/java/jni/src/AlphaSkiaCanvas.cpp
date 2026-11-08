#include "../include/alphaTab_alphaSkia_AlphaSkiaCanvas.h"
#include "../include/JniHelper.h"
#include "../../../../wrapper/include/alphaskia.h"

extern "C"
{
    JNIEXPORT jint JNICALL Java_alphaTab_alphaSkia_AlphaSkiaCanvas_getColor(JNIEnv *env, jobject instance)
    {
        alphaskia_canvas_t canvas = reinterpret_cast<alphaskia_canvas_t>(get_handle(env, instance));
        CHECK_HANDLE_RETURN(canvas, 0)

        return static_cast<jint>(alphaskia_canvas_get_color(canvas));
    }

    JNIEXPORT void JNICALL Java_alphaTab_alphaSkia_AlphaSkiaCanvas_setColor(JNIEnv *env, jobject instance, jint color)
    {
        alphaskia_canvas_t canvas = reinterpret_cast<alphaskia_canvas_t>(get_handle(env, instance));
        CHECK_HANDLE(canvas)

        alphaskia_canvas_set_color(canvas, static_cast<uint32_t>(color));
    }

    JNIEXPORT jfloat JNICALL Java_alphaTab_alphaSkia_AlphaSkiaCanvas_getLineWidth(JNIEnv *env, jobject instance)
    {
        alphaskia_canvas_t canvas = reinterpret_cast<alphaskia_canvas_t>(get_handle(env, instance));
        CHECK_HANDLE_RETURN(canvas, 0.0f)

        return static_cast<jfloat>(alphaskia_canvas_get_line_width(canvas));
    }

    JNIEXPORT void JNICALL Java_alphaTab_alphaSkia_AlphaSkiaCanvas_setLineWidth(JNIEnv *env, jobject instance, jfloat line_width)
    {
        alphaskia_canvas_t canvas = reinterpret_cast<alphaskia_canvas_t>(get_handle(env, instance));
        CHECK_HANDLE(canvas)

        alphaskia_canvas_set_line_width(canvas, static_cast<float>(line_width));
    }

    JNIEXPORT void JNICALL Java_alphaTab_alphaSkia_AlphaSkiaCanvas_release(JNIEnv *, jclass, jlong handle)
    {
        alphaskia_canvas_free(reinterpret_cast<alphaskia_canvas_t>(handle));
    }

    JNIEXPORT void JNICALL Java_alphaTab_alphaSkia_AlphaSkiaCanvas_beginRender(JNIEnv *env, jobject instance, jint width, jint height, jfloat renderScale)
    {
        alphaskia_canvas_t canvas = reinterpret_cast<alphaskia_canvas_t>(get_handle(env, instance));
        CHECK_HANDLE(canvas)
        if (!alphaskia_canvas_begin_render(canvas, width, height, static_cast<float>(renderScale)))
        {
            env->ThrowNew(env->FindClass("java/lang/IllegalArgumentException"), "Cannot create a render surface for the given size and render scale");
        }
    }

    JNIEXPORT void JNICALL Java_alphaTab_alphaSkia_AlphaSkiaCanvas_nativeDrawImage(JNIEnv *env, jobject instance, jobject image, jfloat x, jfloat y, jfloat w, jfloat h)
    {
        alphaskia_canvas_t canvas = reinterpret_cast<alphaskia_canvas_t>(get_handle(env, instance));
        CHECK_HANDLE(canvas)

        alphaskia_image_t nativeImage = reinterpret_cast<alphaskia_image_t>(get_handle(env, image));
        CHECK_HANDLE(nativeImage)

        alphaskia_canvas_draw_image(canvas, nativeImage, static_cast<float>(x), static_cast<float>(y), static_cast<float>(w), static_cast<float>(h));
    }

    JNIEXPORT jobject JNICALL Java_alphaTab_alphaSkia_AlphaSkiaCanvas_endRender(JNIEnv *env, jobject instance)
    {
        alphaskia_canvas_t canvas = reinterpret_cast<alphaskia_canvas_t>(get_handle(env, instance));
        CHECK_HANDLE_RETURN(canvas, nullptr)

        alphaskia_image_t image = alphaskia_canvas_end_render(canvas);
        if (!image)
        {
            return nullptr;
        }

        jclass cls = env->FindClass("alphaTab/alphaSkia/AlphaSkiaImage");
        jmethodID ctor = cls ? env->GetMethodID(cls, "<init>", "(J)V") : nullptr;
        jobject result = ctor ? env->NewObject(cls, ctor, static_cast<jlong>(reinterpret_cast<std::uintptr_t>(image))) : nullptr;
        if (!result)
        {
            alphaskia_image_free(image);
        }
        return result;
    }

    JNIEXPORT void JNICALL Java_alphaTab_alphaSkia_AlphaSkiaCanvas_fillRect(JNIEnv *env, jobject instance, jfloat x, jfloat y, jfloat w, jfloat h)
    {
        alphaskia_canvas_t canvas = reinterpret_cast<alphaskia_canvas_t>(get_handle(env, instance));
        CHECK_HANDLE(canvas)

        alphaskia_canvas_fill_rect(canvas, x, y, w, h);
    }
    JNIEXPORT void JNICALL Java_alphaTab_alphaSkia_AlphaSkiaCanvas_strokeRect(JNIEnv *env, jobject instance, jfloat x, jfloat y, jfloat w, jfloat h)
    {
        alphaskia_canvas_t canvas = reinterpret_cast<alphaskia_canvas_t>(get_handle(env, instance));
        CHECK_HANDLE(canvas)
        alphaskia_canvas_stroke_rect(canvas, x, y, w, h);
    }
    JNIEXPORT void JNICALL Java_alphaTab_alphaSkia_AlphaSkiaCanvas_beginPath(JNIEnv *env, jobject instance)
    {
        alphaskia_canvas_t canvas = reinterpret_cast<alphaskia_canvas_t>(get_handle(env, instance));
        CHECK_HANDLE(canvas)

        alphaskia_canvas_begin_path(canvas);
    }
    JNIEXPORT void JNICALL Java_alphaTab_alphaSkia_AlphaSkiaCanvas_closePath(JNIEnv *env, jobject instance)
    {
        alphaskia_canvas_t canvas = reinterpret_cast<alphaskia_canvas_t>(get_handle(env, instance));
        CHECK_HANDLE(canvas)

        alphaskia_canvas_close_path(canvas);
    }
    JNIEXPORT void JNICALL Java_alphaTab_alphaSkia_AlphaSkiaCanvas_moveTo(JNIEnv *env, jobject instance, jfloat x, jfloat y)
    {
        alphaskia_canvas_t canvas = reinterpret_cast<alphaskia_canvas_t>(get_handle(env, instance));
        CHECK_HANDLE(canvas)

        alphaskia_canvas_move_to(canvas, x, y);
    }
    JNIEXPORT void JNICALL Java_alphaTab_alphaSkia_AlphaSkiaCanvas_lineTo(JNIEnv *env, jobject instance, jfloat x, jfloat y)
    {
        alphaskia_canvas_t canvas = reinterpret_cast<alphaskia_canvas_t>(get_handle(env, instance));
        CHECK_HANDLE(canvas)

        alphaskia_canvas_line_to(canvas, x, y);
    }
    JNIEXPORT void JNICALL Java_alphaTab_alphaSkia_AlphaSkiaCanvas_quadraticCurveTo(JNIEnv *env, jobject instance, jfloat cpx, jfloat cpy, jfloat x, jfloat y)
    {
        alphaskia_canvas_t canvas = reinterpret_cast<alphaskia_canvas_t>(get_handle(env, instance));
        CHECK_HANDLE(canvas)

        alphaskia_canvas_quadratic_curve_to(canvas, cpx, cpy, x, y);
    }
    JNIEXPORT void JNICALL Java_alphaTab_alphaSkia_AlphaSkiaCanvas_bezierCurveTo(JNIEnv *env, jobject instance, jfloat cp1x, jfloat cp1y, jfloat cp2x, jfloat cp2y, jfloat x, jfloat y)
    {
        alphaskia_canvas_t canvas = reinterpret_cast<alphaskia_canvas_t>(get_handle(env, instance));
        CHECK_HANDLE(canvas)

        alphaskia_canvas_bezier_curve_to(canvas, cp1x, cp1y, cp2x, cp2y, x, y);
    }
    JNIEXPORT void JNICALL Java_alphaTab_alphaSkia_AlphaSkiaCanvas_fillCircle(JNIEnv *env, jobject instance, jfloat x, jfloat y, jfloat r)
    {
        alphaskia_canvas_t canvas = reinterpret_cast<alphaskia_canvas_t>(get_handle(env, instance));
        CHECK_HANDLE(canvas)

        alphaskia_canvas_fill_circle(canvas, x, y, r);
    }
    JNIEXPORT void JNICALL Java_alphaTab_alphaSkia_AlphaSkiaCanvas_strokeCircle(JNIEnv *env, jobject instance, jfloat x, jfloat y, jfloat r)
    {
        alphaskia_canvas_t canvas = reinterpret_cast<alphaskia_canvas_t>(get_handle(env, instance));
        CHECK_HANDLE(canvas)

        alphaskia_canvas_stroke_circle(canvas, x, y, r);
    }
    JNIEXPORT void JNICALL Java_alphaTab_alphaSkia_AlphaSkiaCanvas_fill(JNIEnv *env, jobject instance)
    {
        alphaskia_canvas_t canvas = reinterpret_cast<alphaskia_canvas_t>(get_handle(env, instance));
        CHECK_HANDLE(canvas)

        alphaskia_canvas_fill(canvas);
    }
    JNIEXPORT void JNICALL Java_alphaTab_alphaSkia_AlphaSkiaCanvas_stroke(JNIEnv *env, jobject instance)
    {
        alphaskia_canvas_t canvas = reinterpret_cast<alphaskia_canvas_t>(get_handle(env, instance));
        CHECK_HANDLE(canvas)

        alphaskia_canvas_stroke(canvas);
    }
    JNIEXPORT void JNICALL Java_alphaTab_alphaSkia_AlphaSkiaCanvas_nativeFillText(JNIEnv *env, jobject instance, jstring str, jobject text_style, jfloat font_size, jfloat x, jfloat y, jint text_align, jint baseline)
    {
        alphaskia_canvas_t canvas = reinterpret_cast<alphaskia_canvas_t>(get_handle(env, instance));
        CHECK_HANDLE(canvas)

        alphaskia_text_style_t nativeTextStyle = reinterpret_cast<alphaskia_text_style_t>(get_handle(env, text_style));
        CHECK_HANDLE(nativeTextStyle)

        const jsize length = env->GetStringLength(str);
        const jchar *nativeStr = env->GetStringChars(str, nullptr);
        if (!nativeStr)
        {
            return;
        }

        alphaskia_canvas_fill_text(canvas, reinterpret_cast<const char16_t *>(nativeStr), static_cast<int>(length),
                                   nativeTextStyle, static_cast<float>(font_size), static_cast<float>(x), static_cast<float>(y),
                                   static_cast<alphaskia_text_align_t>(text_align), static_cast<alphaskia_text_baseline_t>(baseline));

        env->ReleaseStringChars(str, nativeStr);
    }

    JNIEXPORT jobject JNICALL Java_alphaTab_alphaSkia_AlphaSkiaCanvas_nativeMeasureText(JNIEnv *env, jobject instance, jstring str, jobject text_style, jfloat font_size, jint text_align, jint baseline)
    {
        alphaskia_canvas_t canvas = reinterpret_cast<alphaskia_canvas_t>(get_handle(env, instance));
        CHECK_HANDLE_RETURN(canvas, nullptr)

        alphaskia_text_style_t nativeTextStyle = reinterpret_cast<alphaskia_text_style_t>(get_handle(env, text_style));
        CHECK_HANDLE_RETURN(nativeTextStyle, nullptr)

        const jsize length = env->GetStringLength(str);
        const jchar *nativeStr = env->GetStringChars(str, nullptr);
        if (!nativeStr)
        {
            return nullptr;
        }

        auto text_metrics = alphaskia_canvas_measure_text(canvas, reinterpret_cast<const char16_t *>(nativeStr), static_cast<int>(length), nativeTextStyle, static_cast<float>(font_size),
                                                          static_cast<alphaskia_text_align_t>(text_align), static_cast<alphaskia_text_baseline_t>(baseline));
        env->ReleaseStringChars(str, nativeStr);

        if (!text_metrics)
        {
            env->ThrowNew(env->FindClass("java/lang/IllegalArgumentException"), "Cannot measure text with the given arguments");
            return nullptr;
        }

        jclass cls = env->FindClass("alphaTab/alphaSkia/AlphaSkiaTextMetrics");
        jmethodID ctor = cls ? env->GetMethodID(cls, "<init>", "(J)V") : nullptr;
        jobject result = ctor ? env->NewObject(cls, ctor, static_cast<jlong>(reinterpret_cast<std::uintptr_t>(text_metrics))) : nullptr;
        if (!result)
        {
            alphaskia_text_metrics_free(text_metrics);
        }
        return result;
    }

    JNIEXPORT void JNICALL Java_alphaTab_alphaSkia_AlphaSkiaCanvas_beginRotate(JNIEnv *env, jobject instance, jfloat x, jfloat y, jfloat angle)
    {
        alphaskia_canvas_t canvas = reinterpret_cast<alphaskia_canvas_t>(get_handle(env, instance));
        CHECK_HANDLE(canvas)

        alphaskia_canvas_begin_rotate(canvas, static_cast<float>(x), static_cast<float>(y), static_cast<float>(angle));
    }
    JNIEXPORT void JNICALL Java_alphaTab_alphaSkia_AlphaSkiaCanvas_endRotate(JNIEnv *env, jobject instance)
    {
        alphaskia_canvas_t canvas = reinterpret_cast<alphaskia_canvas_t>(get_handle(env, instance));
        CHECK_HANDLE(canvas)
        alphaskia_canvas_end_rotate(canvas);
    }
    JNIEXPORT jlong JNICALL Java_alphaTab_alphaSkia_AlphaSkiaCanvas_alphaskiaCanvasAllocate(JNIEnv *env, jclass)
    {
        alphaskia_canvas_t canvas = alphaskia_canvas_new();
        return static_cast<jlong>(reinterpret_cast<std::uintptr_t>(canvas));
    }
    JNIEXPORT jint JNICALL Java_alphaTab_alphaSkia_AlphaSkiaCanvas_alphaskiaColorType(JNIEnv *, jclass)
    {
        return static_cast<jint>(alphaskia_get_color_type());
    }

    JNIEXPORT void JNICALL Java_alphaTab_alphaSkia_AlphaSkiaCanvas_switchToFreeTypeFonts(JNIEnv *, jclass) {
        alphaskia_switch_to_freetype_fonts();
    }

    JNIEXPORT void JNICALL Java_alphaTab_alphaSkia_AlphaSkiaCanvas_switchToOperatingSystemFonts(JNIEnv *, jclass) {
        alphaskia_switch_to_operating_system_fonts();
    }
}
