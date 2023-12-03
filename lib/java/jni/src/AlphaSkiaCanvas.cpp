#include "../include/alphaTab_alphaSkia_AlphaSkiaCanvas.h"
#include "../include/JniHelper.h"
#include "../../../../wrapper/include/alphaskia.h"

extern "C"
{
    JNIEXPORT jint JNICALL Java_alphaTab_alphaSkia_AlphaSkiaCanvas_getColor(JNIEnv *env, jobject instance)
    {
        SCOPE_LOG()
        alphaskia_canvas_t canvas = reinterpret_cast<alphaskia_canvas_t>(get_handle(env, instance));
        CHECK_HANDLE_RETURN(canvas, 0)

        return static_cast<jint>(alphaskia_canvas_get_color(canvas));
    }

    JNIEXPORT void JNICALL Java_alphaTab_alphaSkia_AlphaSkiaCanvas_setColor(JNIEnv *env, jobject instance, jint color)
    {
        SCOPE_LOG()
        alphaskia_canvas_t canvas = reinterpret_cast<alphaskia_canvas_t>(get_handle(env, instance));
        CHECK_HANDLE(canvas)

        alphaskia_canvas_set_color(canvas, static_cast<uint32_t>(color));
    }

    JNIEXPORT jfloat JNICALL Java_alphaTab_alphaSkia_AlphaSkiaCanvas_getLineWidth(JNIEnv *env, jobject instance)
    {
        SCOPE_LOG()
        alphaskia_canvas_t canvas = reinterpret_cast<alphaskia_canvas_t>(get_handle(env, instance));
        CHECK_HANDLE_RETURN(canvas, 0.0f)

        return static_cast<jfloat>(alphaskia_canvas_get_line_width(canvas));
    }

    JNIEXPORT void JNICALL Java_alphaTab_alphaSkia_AlphaSkiaCanvas_setLineWidth(JNIEnv *env, jobject instance, jfloat line_width)
    {
        SCOPE_LOG()
        alphaskia_canvas_t canvas = reinterpret_cast<alphaskia_canvas_t>(get_handle(env, instance));
        CHECK_HANDLE(canvas)

        alphaskia_canvas_set_line_width(canvas, static_cast<float>(line_width));
    }

    JNIEXPORT void JNICALL Java_alphaTab_alphaSkia_AlphaSkiaCanvas_close(JNIEnv *env, jobject instance)
    {
        SCOPE_LOG()
        alphaskia_canvas_t canvas = reinterpret_cast<alphaskia_canvas_t>(get_handle(env, instance));
        CHECK_HANDLE(canvas)

        alphaskia_canvas_free(canvas);
        set_handle(env, instance, 0);
    }

    JNIEXPORT void JNICALL Java_alphaTab_alphaSkia_AlphaSkiaCanvas_beginRender(JNIEnv *env, jobject instance, jint width, jint height, jfloat renderScale)
    {
        SCOPE_LOG()
        alphaskia_canvas_t canvas = reinterpret_cast<alphaskia_canvas_t>(get_handle(env, instance));
        CHECK_HANDLE(canvas)
        alphaskia_canvas_begin_render(canvas, width, height, static_cast<float>(renderScale));
    }

    JNIEXPORT void JNICALL Java_alphaTab_alphaSkia_AlphaSkiaCanvas_drawImage(JNIEnv *env, jobject instance, jobject image, jfloat x, jfloat y, jfloat w, jfloat h)
    {
        SCOPE_LOG()
        alphaskia_canvas_t canvas = reinterpret_cast<alphaskia_canvas_t>(get_handle(env, instance));
        CHECK_HANDLE(canvas)

        alphaskia_image_t nativeImage = reinterpret_cast<alphaskia_image_t>(get_handle(env, image));
        if (!canvas)
        {
            if (!env->ExceptionCheck())
            {
                env->ThrowNew(env->FindClass("java/lang/IllegalArgumentException"), "Invalid object handle");
            }
            return;
        }

        alphaskia_canvas_draw_image(canvas, nativeImage, static_cast<float>(x), static_cast<float>(y), static_cast<float>(w), static_cast<float>(h));
    }

    JNIEXPORT jobject JNICALL Java_alphaTab_alphaSkia_AlphaSkiaCanvas_endRender(JNIEnv *env, jobject instance)
    {
        SCOPE_LOG()
        alphaskia_canvas_t canvas = reinterpret_cast<alphaskia_canvas_t>(get_handle(env, instance));
        CHECK_HANDLE_RETURN(canvas, nullptr)

        alphaskia_image_t image = alphaskia_canvas_end_render(canvas);

        jclass cls = env->FindClass("alphaTab/alphaSkia/AlphaSkiaImage");
        jmethodID ctor = env->GetMethodID(cls, "<init>", "(J)V");
        return env->NewObject(cls, ctor, static_cast<jlong>(reinterpret_cast<std::uintptr_t>(image)));
    }

    JNIEXPORT void JNICALL Java_alphaTab_alphaSkia_AlphaSkiaCanvas_fillRect(JNIEnv *env, jobject instance, jfloat x, jfloat y, jfloat w, jfloat h)
    {
        SCOPE_LOG()
        alphaskia_canvas_t canvas = reinterpret_cast<alphaskia_canvas_t>(get_handle(env, instance));
        CHECK_HANDLE(canvas)

        alphaskia_canvas_fill_rect(canvas, x, y, w, h);
    }
    JNIEXPORT void JNICALL Java_alphaTab_alphaSkia_AlphaSkiaCanvas_strokeRect(JNIEnv *env, jobject instance, jfloat x, jfloat y, jfloat w, jfloat h)
    {
        SCOPE_LOG()
        alphaskia_canvas_t canvas = reinterpret_cast<alphaskia_canvas_t>(get_handle(env, instance));
        CHECK_HANDLE(canvas)
        alphaskia_canvas_stroke_rect(canvas, x, y, w, h);
    }
    JNIEXPORT void JNICALL Java_alphaTab_alphaSkia_AlphaSkiaCanvas_beginPath(JNIEnv *env, jobject instance)
    {
        SCOPE_LOG()
        alphaskia_canvas_t canvas = reinterpret_cast<alphaskia_canvas_t>(get_handle(env, instance));
        CHECK_HANDLE(canvas)

        alphaskia_canvas_begin_path(canvas);
    }
    JNIEXPORT void JNICALL Java_alphaTab_alphaSkia_AlphaSkiaCanvas_closePath(JNIEnv *env, jobject instance)
    {
        SCOPE_LOG()
        alphaskia_canvas_t canvas = reinterpret_cast<alphaskia_canvas_t>(get_handle(env, instance));
        CHECK_HANDLE(canvas)

        alphaskia_canvas_close_path(canvas);
    }
    JNIEXPORT void JNICALL Java_alphaTab_alphaSkia_AlphaSkiaCanvas_moveTo(JNIEnv *env, jobject instance, jfloat x, jfloat y)
    {
        SCOPE_LOG()
        alphaskia_canvas_t canvas = reinterpret_cast<alphaskia_canvas_t>(get_handle(env, instance));
        CHECK_HANDLE(canvas)

        alphaskia_canvas_move_to(canvas, x, y);
    }
    JNIEXPORT void JNICALL Java_alphaTab_alphaSkia_AlphaSkiaCanvas_lineTo(JNIEnv *env, jobject instance, jfloat x, jfloat y)
    {
        SCOPE_LOG()
        alphaskia_canvas_t canvas = reinterpret_cast<alphaskia_canvas_t>(get_handle(env, instance));
        CHECK_HANDLE(canvas)

        alphaskia_canvas_line_to(canvas, x, y);
    }
    JNIEXPORT void JNICALL Java_alphaTab_alphaSkia_AlphaSkiaCanvas_quadraticCurveTo(JNIEnv *env, jobject instance, jfloat cpx, jfloat cpy, jfloat x, jfloat y)
    {
        SCOPE_LOG()
        alphaskia_canvas_t canvas = reinterpret_cast<alphaskia_canvas_t>(get_handle(env, instance));
        CHECK_HANDLE(canvas)

        alphaskia_canvas_quadratic_curve_to(canvas, cpx, cpy, x, y);
    }
    JNIEXPORT void JNICALL Java_alphaTab_alphaSkia_AlphaSkiaCanvas_bezierCurveTo(JNIEnv *env, jobject instance, jfloat cp1x, jfloat cp1y, jfloat cp2x, jfloat cp2y, jfloat x, jfloat y)
    {
        SCOPE_LOG()
        alphaskia_canvas_t canvas = reinterpret_cast<alphaskia_canvas_t>(get_handle(env, instance));
        CHECK_HANDLE(canvas)

        alphaskia_canvas_bezier_curve_to(canvas, cp1x, cp1y, cp2x, cp2y, x, y);
    }
    JNIEXPORT void JNICALL Java_alphaTab_alphaSkia_AlphaSkiaCanvas_fillCircle(JNIEnv *env, jobject instance, jfloat x, jfloat y, jfloat r)
    {
        SCOPE_LOG()
        alphaskia_canvas_t canvas = reinterpret_cast<alphaskia_canvas_t>(get_handle(env, instance));
        CHECK_HANDLE(canvas)

        alphaskia_canvas_fill_circle(canvas, x, y, r);
    }
    JNIEXPORT void JNICALL Java_alphaTab_alphaSkia_AlphaSkiaCanvas_strokeCircle(JNIEnv *env, jobject instance, jfloat x, jfloat y, jfloat r)
    {
        SCOPE_LOG()
        alphaskia_canvas_t canvas = reinterpret_cast<alphaskia_canvas_t>(get_handle(env, instance));
        CHECK_HANDLE(canvas)

        alphaskia_canvas_stroke_circle(canvas, x, y, r);
    }
    JNIEXPORT void JNICALL Java_alphaTab_alphaSkia_AlphaSkiaCanvas_fill(JNIEnv *env, jobject instance)
    {
        SCOPE_LOG()
        alphaskia_canvas_t canvas = reinterpret_cast<alphaskia_canvas_t>(get_handle(env, instance));
        CHECK_HANDLE(canvas)

        alphaskia_canvas_fill(canvas);
    }
    JNIEXPORT void JNICALL Java_alphaTab_alphaSkia_AlphaSkiaCanvas_stroke(JNIEnv *env, jobject instance)
    {
        SCOPE_LOG()
        alphaskia_canvas_t canvas = reinterpret_cast<alphaskia_canvas_t>(get_handle(env, instance));
        CHECK_HANDLE(canvas)

        alphaskia_canvas_stroke(canvas);
    }
    JNIEXPORT void JNICALL Java_alphaTab_alphaSkia_AlphaSkiaCanvas_fillText(JNIEnv *env, jobject instance, jstring str, jobject typeface, jfloat font_size, jfloat x, jfloat y, jobject text_align, jobject baseline)
    {
        SCOPE_LOG()
        alphaskia_canvas_t canvas = reinterpret_cast<alphaskia_canvas_t>(get_handle(env, instance));
        CHECK_HANDLE(canvas)

        const jchar *nativeStr = env->GetStringChars(str, nullptr);

        alphaskia_typeface_t nativeTypeface = reinterpret_cast<alphaskia_typeface_t>(get_handle(env, typeface));
        CHECK_HANDLE(nativeTypeface)

        jmethodID textAlignGetValue = env->GetMethodID(env->GetObjectClass(text_align), "getValue", "()I");
        if (!textAlignGetValue)
        {
            if (!env->ExceptionCheck())
            {
                env->ThrowNew(env->FindClass("java/lang/IllegalStateException"), "Could not find 'int getValue' on text align");
            }
            return;
        }

        jmethodID baselineGetValue = env->GetMethodID(env->GetObjectClass(baseline), "getValue", "()I");
        if (!baselineGetValue)
        {
            if (!env->ExceptionCheck())
            {
                env->ThrowNew(env->FindClass("java/lang/IllegalStateException"), "Could not find 'int getValue' on text baseline");
            }
            return;
        }

        alphaskia_text_align_t nativeTextAlign = static_cast<alphaskia_text_align_t>(env->CallIntMethod(text_align, textAlignGetValue));
        alphaskia_text_baseline_t nativeBaseline = static_cast<alphaskia_text_baseline_t>(env->CallIntMethod(baseline, baselineGetValue));

        alphaskia_canvas_fill_text(canvas, reinterpret_cast<const char16_t *>(nativeStr), nativeTypeface, static_cast<float>(font_size), static_cast<float>(x), static_cast<float>(y),
                                   nativeTextAlign, nativeBaseline);

        env->ReleaseStringChars(str, nativeStr);
    }
    JNIEXPORT jfloat JNICALL Java_alphaTab_alphaSkia_AlphaSkiaCanvas_measureText(JNIEnv *env, jobject instance, jstring str, jobject typeface, jfloat font_size)
    {
        SCOPE_LOG()
        alphaskia_canvas_t canvas = reinterpret_cast<alphaskia_canvas_t>(get_handle(env, instance));
        CHECK_HANDLE_RETURN(canvas, 0.0f)

        const jchar *nativeStr = env->GetStringChars(str, nullptr);
        alphaskia_typeface_t nativeTypeface = reinterpret_cast<alphaskia_typeface_t>(get_handle(env, typeface));
        CHECK_HANDLE_RETURN(nativeTypeface, 0.0f)

        float width = alphaskia_canvas_measure_text(canvas, reinterpret_cast<const char16_t *>(nativeStr), nativeTypeface, static_cast<float>(font_size));

        env->ReleaseStringChars(str, nativeStr);

        return static_cast<jfloat>(width);
    }

    JNIEXPORT void JNICALL Java_alphaTab_alphaSkia_AlphaSkiaCanvas_beginRotate(JNIEnv *env, jobject instance, jfloat x, jfloat y, jfloat angle)
    {
        SCOPE_LOG()
        alphaskia_canvas_t canvas = reinterpret_cast<alphaskia_canvas_t>(get_handle(env, instance));
        CHECK_HANDLE(canvas)

        alphaskia_canvas_begin_rotate(canvas, static_cast<float>(x), static_cast<float>(y), static_cast<float>(angle));
    }
    JNIEXPORT void JNICALL Java_alphaTab_alphaSkia_AlphaSkiaCanvas_endRotate(JNIEnv *env, jobject instance)
    {
        SCOPE_LOG()
        alphaskia_canvas_t canvas = reinterpret_cast<alphaskia_canvas_t>(get_handle(env, instance));
        CHECK_HANDLE(canvas)
        alphaskia_canvas_end_rotate(canvas);
    }
    JNIEXPORT jlong JNICALL Java_alphaTab_alphaSkia_AlphaSkiaCanvas_alphaskiaCanvasAllocate(JNIEnv *env, jclass)
    {
        SCOPE_LOG()
        alphaskia_canvas_t canvas = alphaskia_canvas_new();
        return static_cast<jlong>(reinterpret_cast<std::uintptr_t>(canvas));
    }
    JNIEXPORT jint JNICALL Java_alphaTab_alphaSkia_AlphaSkiaCanvas_alphaskiaColorType(JNIEnv *, jclass)
    {
        SCOPE_LOG()
        return static_cast<jint>(alphaskia_get_color_type());
    }
}