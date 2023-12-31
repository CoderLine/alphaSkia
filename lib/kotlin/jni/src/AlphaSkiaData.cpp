#include "../include/alphaTab_alphaSkia_NativeMethods.h"
#include "../../../../wrapper/include/alphaskia.h"

#include <cstring>

extern "C"
{
    JNIEXPORT jlong JNICALL Java_alphaTab_alphaSkia_NativeMethods_alphaskiaDataNewCopy(JNIEnv * env, jclass clz, jbyteArray data)
    {
        jbyte *bytes = env->GetByteArrayElements(data, nullptr);
        alphaskia_data_t nativeData = alphaskia_data_new_copy(reinterpret_cast<const uint8_t *>(bytes), env->GetArrayLength(data));
        env->ReleaseByteArrayElements(data, bytes, 0);
        return reinterpret_cast<jlong>(nativeData);
    }

    JNIEXPORT jbyteArray JNICALL Java_alphaTab_alphaSkia_NativeMethods_alphaskiaDataGetData(JNIEnv * env, jclass clz, jlong handle)
    {
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

    JNIEXPORT void JNICALL Java_alphaTab_alphaSkia_NativeMethods_alphaskiaDataFree(JNIEnv * env, jclass clz, jlong handle)
    {
        alphaskia_data_t data = reinterpret_cast<alphaskia_data_t>(handle);
        alphaskia_data_free(data);
    }
}
