#include "../include/alphaTab_alphaSkia_AlphaSkiaData.h"
#include "../include/JniHelper.h"
#include "../../../../wrapper/include/alphaskia.h"

#include <cstdint>

extern "C"
{
    JNIEXPORT jlong JNICALL Java_alphaTab_alphaSkia_AlphaSkiaData_allocateCopy(JNIEnv *env, jclass cls, jbyteArray data)
    {
        if (!data)
        {
            env->ThrowNew(env->FindClass("java/lang/NullPointerException"), "data");
            return 0;
        }

        jbyte *bytes = env->GetByteArrayElements(data, nullptr);
        if (!bytes)
        {
            return 0;
        }

        alphaskia_data_t nativeData = alphaskia_data_new_copy(reinterpret_cast<const uint8_t *>(bytes), static_cast<uint64_t>(env->GetArrayLength(data)));
        env->ReleaseByteArrayElements(data, bytes, JNI_ABORT);
        return reinterpret_cast<jlong>(nativeData);
    }
    
    JNIEXPORT void JNICALL Java_alphaTab_alphaSkia_AlphaSkiaData_close(JNIEnv *env, jobject instance)
    {
        jlong handle = get_handle(env, instance);
        CHECK_HANDLE(handle)

        alphaskia_data_t data = reinterpret_cast<alphaskia_data_t>(handle);
        alphaskia_data_free(data);
        set_handle(env, instance, 0);
    }

    JNIEXPORT jbyteArray JNICALL Java_alphaTab_alphaSkia_AlphaSkiaData_toArray(JNIEnv *env, jobject instance)
    {
        jlong handle = get_handle(env, instance);
        CHECK_HANDLE_RETURN(handle, nullptr)

        alphaskia_data_t data = reinterpret_cast<alphaskia_data_t>(handle);
        const uint64_t byteCount = alphaskia_data_get_length(data);
        if (byteCount > INT32_MAX)
        {
            env->ThrowNew(env->FindClass("java/lang/IllegalStateException"), "Data too large for a Java byte array");
            return nullptr;
        }

        jbyteArray array = env->NewByteArray(static_cast<jsize>(byteCount));
        if (array)
        {
            env->SetByteArrayRegion(array, 0, static_cast<jsize>(byteCount), reinterpret_cast<const jbyte *>(alphaskia_data_get_data(data)));
        }
        return array;
    }
}
