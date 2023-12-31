#include "../include/alphaTab_alphaSkia_NativeMethods.h"
#include "../../../../wrapper/include/alphaskia.h"

#include <cstring>

extern "C"
{
    JNIEXPORT jint JNICALL Java_alphaTab_alphaSkia_NativeMethods_alphaskiaImageGetWidth(JNIEnv * env, jclass clz, jlong handle)
    {
        return alphaskia_image_get_width(reinterpret_cast<alphaskia_image_t>(handle));
    }

    JNIEXPORT jint JNICALL Java_alphaTab_alphaSkia_NativeMethods_alphaskiaImageGetHeight(JNIEnv * env, jclass clz, jlong handle)
    {
        return alphaskia_image_get_height(reinterpret_cast<alphaskia_image_t>(handle));
    }

    JNIEXPORT void JNICALL Java_alphaTab_alphaSkia_NativeMethods_alphaskiaImageFree(JNIEnv * env, jclass clz, jlong handle)
    {
        alphaskia_image_free(reinterpret_cast<alphaskia_image_t>(handle));
    }

    JNIEXPORT jbyteArray JNICALL Java_alphaTab_alphaSkia_NativeMethods_alphaskiaImageReadPixels(JNIEnv * env, jclass clz, jlong handle)
    {
        alphaskia_image_t image = reinterpret_cast<alphaskia_image_t>(handle);
        jsize rowBytes = alphaskia_image_get_width(image) * sizeof(int32_t);
        jbyteArray pixels = env->NewByteArray(
            rowBytes * alphaskia_image_get_height(image));
        jbyte *bytes = env->GetByteArrayElements(pixels, nullptr);

        if (alphaskia_image_read_pixels(image, reinterpret_cast<uint8_t *>(bytes), rowBytes) == 0)
        {
            env->ReleaseByteArrayElements(pixels, bytes, 0);
            return nullptr;
        }

        env->ReleaseByteArrayElements(pixels, bytes, 0);
        return pixels;
    }

    JNIEXPORT jbyteArray JNICALL Java_alphaTab_alphaSkia_NativeMethods_alphaskiaImageEncodePng(JNIEnv * env, jclass clz, jlong handle)
    {
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

    JNIEXPORT jlong JNICALL Java_alphaTab_alphaSkia_NativeMethods_alphaskiaImageFromPixels(JNIEnv * env, jclass clz, jint width, jint height, jbyteArray pixels)
    {
        jbyte *bytes = env->GetByteArrayElements(pixels, nullptr);
        alphaskia_image_t nativeImage = alphaskia_image_from_pixels(static_cast<int32_t>(width), static_cast<int32_t>(height), reinterpret_cast<const uint8_t *>(bytes));
        env->ReleaseByteArrayElements(pixels, bytes, 0);
        return reinterpret_cast<jlong>(nativeImage);
    }

    JNIEXPORT jlong JNICALL Java_alphaTab_alphaSkia_NativeMethods_alphaskiaImageDecode(JNIEnv * env, jclass clz, jbyteArray bytes)
    {
        jbyte *raw = env->GetByteArrayElements(bytes, nullptr);
        alphaskia_image_t nativeImage = alphaskia_image_decode(reinterpret_cast<const uint8_t *>(raw), env->GetArrayLength(bytes));
        env->ReleaseByteArrayElements(bytes, raw, 0);
        return reinterpret_cast<jlong>(nativeImage);
    }
}