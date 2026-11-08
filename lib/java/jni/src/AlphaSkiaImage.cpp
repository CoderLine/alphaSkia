#include "../include/alphaTab_alphaSkia_AlphaSkiaImage.h"
#include "../include/JniHelper.h"
#include "../../../../wrapper/include/alphaskia.h"

#include <cstdint>

extern "C"
{
    JNIEXPORT jint JNICALL Java_alphaTab_alphaSkia_AlphaSkiaImage_getWidth(JNIEnv *env, jobject instance)
    {
        jlong handle = get_handle(env, instance);
        CHECK_HANDLE_RETURN(handle, 0)
        return alphaskia_image_get_width(reinterpret_cast<alphaskia_image_t>(handle));
    }

    JNIEXPORT jint JNICALL Java_alphaTab_alphaSkia_AlphaSkiaImage_getHeight(JNIEnv *env, jobject instance)
    {
        jlong handle = get_handle(env, instance);
        CHECK_HANDLE_RETURN(handle, 0)

        return alphaskia_image_get_height(reinterpret_cast<alphaskia_image_t>(handle));
    }

    JNIEXPORT void JNICALL Java_alphaTab_alphaSkia_AlphaSkiaImage_release(JNIEnv *, jclass, jlong handle)
    {
        alphaskia_image_free(reinterpret_cast<alphaskia_image_t>(handle));
    }

    JNIEXPORT jbyteArray JNICALL Java_alphaTab_alphaSkia_AlphaSkiaImage_readPixels(JNIEnv *env, jobject instance)
    {
        jlong handle = get_handle(env, instance);
        CHECK_HANDLE_RETURN(handle, nullptr)

        alphaskia_image_t image = reinterpret_cast<alphaskia_image_t>(handle);
        const uint64_t rowBytes = static_cast<uint64_t>(alphaskia_image_get_width(image)) * sizeof(int32_t);
        const uint64_t byteCount = rowBytes * static_cast<uint64_t>(alphaskia_image_get_height(image));
        if (byteCount > INT32_MAX)
        {
            env->ThrowNew(env->FindClass("java/lang/IllegalStateException"), "Image too large for a Java byte array");
            return nullptr;
        }

        jbyteArray pixels = env->NewByteArray(static_cast<jsize>(byteCount));
        if (!pixels)
        {
            return nullptr;
        }

        jbyte *bytes = env->GetByteArrayElements(pixels, nullptr);
        if (!bytes)
        {
            return nullptr;
        }

        const uint8_t success = alphaskia_image_read_pixels(image, reinterpret_cast<uint8_t *>(bytes), rowBytes, byteCount);
        env->ReleaseByteArrayElements(pixels, bytes, success ? 0 : JNI_ABORT);
        return success ? pixels : nullptr;
    }

    JNIEXPORT jbyteArray JNICALL Java_alphaTab_alphaSkia_AlphaSkiaImage_toPng(JNIEnv *env, jobject instance)
    {
        jlong handle = get_handle(env, instance);
        CHECK_HANDLE_RETURN(handle, nullptr)

        alphaskia_image_t image = reinterpret_cast<alphaskia_image_t>(handle);
        alphaskia_data_t data = alphaskia_image_encode_png(image);
        if (!data)
        {
            return nullptr;
        }

        const uint64_t byteCount = alphaskia_data_get_length(data);
        if (byteCount > INT32_MAX)
        {
            alphaskia_data_free(data);
            env->ThrowNew(env->FindClass("java/lang/IllegalStateException"), "PNG too large for a Java byte array");
            return nullptr;
        }

        jbyteArray png = env->NewByteArray(static_cast<jsize>(byteCount));
        if (png)
        {
            env->SetByteArrayRegion(png, 0, static_cast<jsize>(byteCount), reinterpret_cast<const jbyte *>(alphaskia_data_get_data(data)));
        }
        alphaskia_data_free(data);

        return png;
    }

    JNIEXPORT jlong JNICALL Java_alphaTab_alphaSkia_AlphaSkiaImage_createFromPixels(JNIEnv *env, jclass clz, jint width, jint height, jbyteArray pixels)
    {
        if (!pixels)
        {
            env->ThrowNew(env->FindClass("java/lang/NullPointerException"), "pixels");
            return 0;
        }

        jbyte *bytes = env->GetByteArrayElements(pixels, nullptr);
        if (!bytes)
        {
            return 0;
        }

        alphaskia_image_t nativeImage = alphaskia_image_from_pixels(static_cast<int32_t>(width), static_cast<int32_t>(height), reinterpret_cast<const uint8_t *>(bytes), static_cast<uint64_t>(env->GetArrayLength(pixels)));
        env->ReleaseByteArrayElements(pixels, bytes, JNI_ABORT);
        return reinterpret_cast<jlong>(nativeImage);
    }

    JNIEXPORT jlong JNICALL Java_alphaTab_alphaSkia_AlphaSkiaImage_allocateDecoded(JNIEnv *env, jclass clz, jbyteArray bytes)
    {
        if (!bytes)
        {
            env->ThrowNew(env->FindClass("java/lang/NullPointerException"), "bytes");
            return 0;
        }

        jbyte *raw = env->GetByteArrayElements(bytes, nullptr);
        if (!raw)
        {
            return 0;
        }

        alphaskia_image_t nativeImage = alphaskia_image_decode(reinterpret_cast<const uint8_t *>(raw), static_cast<uint64_t>(env->GetArrayLength(bytes)));
        env->ReleaseByteArrayElements(bytes, raw, JNI_ABORT);
        return reinterpret_cast<jlong>(nativeImage);
    }
}