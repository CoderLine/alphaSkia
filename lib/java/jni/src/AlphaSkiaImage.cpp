#include "../include/net_alphatab_alphaskia_AlphaSkiaImage.h"
#include "../include/JniHelper.h"
#include "../../../../wrapper/include/alphaskia.h"

#include <cstring>

extern "C"
{
    JNIEXPORT jint JNICALL Java_net_alphatab_alphaskia_AlphaSkiaImage_getWidth(JNIEnv *env, jobject instance)
    {
        jlong handle = get_handle(env, instance);
        return alphaskia_image_get_width(reinterpret_cast<alphaskia_image_t>(handle));
    }

    JNIEXPORT jint JNICALL Java_net_alphatab_alphaskia_AlphaSkiaImage_getHeight(JNIEnv *env, jobject instance)
    {
        jlong handle = get_handle(env, instance);
        return alphaskia_image_get_height(reinterpret_cast<alphaskia_image_t>(handle));
    }

    JNIEXPORT void JNICALL Java_net_alphatab_alphaskia_AlphaSkiaImage_close(JNIEnv *env, jobject instance)
    {
        jlong handle = get_handle(env, instance);
        alphaskia_image_free(reinterpret_cast<alphaskia_image_t>(handle));
        set_handle(env, instance, 0);
    }

    JNIEXPORT jbyteArray JNICALL Java_net_alphatab_alphaskia_AlphaSkiaImage_readPixels(JNIEnv *env, jobject instance)
    {
        jlong handle = get_handle(env, instance);
        alphaskia_image_t image = reinterpret_cast<alphaskia_image_t>(handle);
        jsize rowBytes = alphaskia_image_get_width(image) * sizeof(int32_t);
        jbyteArray pixels = env->NewByteArray(
            rowBytes * alphaskia_image_get_height(image));
        jbyte *bytes = env->GetByteArrayElements(pixels, nullptr);

        if ( alphaskia_image_read_pixels(image, reinterpret_cast<uint8_t *>(bytes), rowBytes) == 0)
        {
            env->ReleaseByteArrayElements(pixels, bytes, 0);
            return nullptr;
        }

        env->ReleaseByteArrayElements(pixels, bytes, 0);
        return pixels;
    }

    JNIEXPORT jbyteArray JNICALL Java_net_alphatab_alphaskia_AlphaSkiaImage_toPng(JNIEnv *env, jobject instance)
    {
        jlong handle = get_handle(env, instance);
        alphaskia_image_t image = reinterpret_cast<alphaskia_image_t>(handle);
        alphaskia_data_t data = alphaskia_image_encode_png(image);
        uint8_t *raw = alphaskia_data_get_data(data);

        uint64_t byteCount = alphaskia_data_get_length(data);
        jbyteArray png = env->NewByteArray(byteCount);
        jbyte *bytes = env->GetByteArrayElements(png, nullptr);

        memcpy(bytes, raw, byteCount);

        env->ReleaseByteArrayElements(png, bytes, 0);
        alphaskia_data_free(data);

        return png;
    }

    JNIEXPORT jlong JNICALL Java_net_alphatab_alphaskia_AlphaSkiaImage_createFromPixels(JNIEnv *env, jclass clz, jint width, jint height, jbyteArray pixels)
    {
        jbyte *bytes = env->GetByteArrayElements(pixels, nullptr);
        alphaskia_image_t nativeImage = alphaskia_image_from_pixels(static_cast<int32_t>(width), static_cast<int32_t>(height), reinterpret_cast<const uint8_t *>(bytes));
        env->ReleaseByteArrayElements(pixels, bytes, 0);
        return reinterpret_cast<jlong>(nativeImage);
    }

    JNIEXPORT jlong JNICALL Java_net_alphatab_alphaskia_AlphaSkiaImage_allocateDecoded(JNIEnv *env, jclass clz, jbyteArray bytes)
    {
        jbyte *raw = env->GetByteArrayElements(bytes, nullptr);
        alphaskia_image_t nativeImage = alphaskia_image_decode(reinterpret_cast<const uint8_t *>(raw), env->GetArrayLength(bytes));
        env->ReleaseByteArrayElements(bytes, raw, 0);
        return reinterpret_cast<jlong>(nativeImage);
    }
}