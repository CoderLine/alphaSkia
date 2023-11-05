#include "../include/alphaTab_alphaSkia_AlphaSkiaData.h"
#include "../include/JniHelper.h"
#include "../../../../wrapper/include/alphaskia.h"

#include <cstring>

extern "C"
{
    JNIEXPORT jlong JNICALL Java_alphaTab_alphaSkia_AlphaSkiaData_allocateCopy(JNIEnv *env, jclass cls, jbyteArray data)
    {
        jbyte *bytes = env->GetByteArrayElements(data, nullptr);
        alphaskia_data_t nativeData = alphaskia_data_new_copy(reinterpret_cast<const uint8_t *>(bytes), env->GetArrayLength(data));
        env->ReleaseByteArrayElements(data, bytes, 0);
        return reinterpret_cast<jlong>(nativeData);
    }
    
    JNIEXPORT void JNICALL Java_alphaTab_alphaSkia_AlphaSkiaData_close(JNIEnv *env, jobject instance)
    {
        jlong handle = get_handle(env, instance);
        alphaskia_data_t data = reinterpret_cast<alphaskia_data_t>(handle);
        alphaskia_data_free(data);
        set_handle(env, instance, 0);
    }

    JNIEXPORT jbyteArray JNICALL Java_alphaTab_alphaSkia_AlphaSkiaData_toArray(JNIEnv *env, jobject instance)
    {
        jlong handle = get_handle(env, instance);
        alphaskia_data_t data = reinterpret_cast<alphaskia_data_t>(handle);
        uint8_t *raw = alphaskia_data_get_data(data);

        uint64_t byteCount = alphaskia_data_get_length(data);
        jbyteArray png = env->NewByteArray(byteCount);
        jbyte *bytes = env->GetByteArrayElements(png, nullptr);

        memcpy(bytes, raw, byteCount);

        env->ReleaseByteArrayElements(png, bytes, 0);
        alphaskia_data_free(data);

        return png;
    }
}
