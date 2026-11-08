#include <jni.h>

#include <cstdint>
#include <string>

#define STRINGIFY(s) _STRINGIFY(s)
#define _STRINGIFY(s) #s

// Returns 0 with a pending exception if the class or field cannot be found.
inline jlong get_handle(JNIEnv *env, jobject instance)
{
    if (!instance)
    {
        env->ThrowNew(env->FindClass("java/lang/NullPointerException"), "object is null");
        return 0;
    }
    jclass cls = env->FindClass("alphaTab/alphaSkia/AlphaSkiaNative");
    if (!cls)
    {
        return 0;
    }
    jfieldID handleFieldId = env->GetFieldID(cls, "handle", "J");
    if (!handleFieldId)
    {
        return 0;
    }
    return env->GetLongField(instance, handleFieldId);
}

// Copies a Java byte[] holding UTF-8 into a NUL terminated string.
inline std::string utf8_from_bytes(JNIEnv *env, jbyteArray bytes)
{
    const jsize length = env->GetArrayLength(bytes);
    std::string result(static_cast<size_t>(length), '\0');
    env->GetByteArrayRegion(bytes, 0, length, reinterpret_cast<jbyte *>(result.data()));
    return result;
}

// Returns a Java byte[] with the given UTF-8 bytes, or nullptr with a pending exception.
inline jbyteArray bytes_from_utf8(JNIEnv *env, const char *utf8, size_t length)
{
    if (length > INT32_MAX)
    {
        env->ThrowNew(env->FindClass("java/lang/IllegalStateException"), "String too long for a Java byte array");
        return nullptr;
    }
    jbyteArray result = env->NewByteArray(static_cast<jsize>(length));
    if (result && length > 0)
    {
        env->SetByteArrayRegion(result, 0, static_cast<jsize>(length), reinterpret_cast<const jbyte *>(utf8));
    }
    return result;
}

#define CHECK_HANDLE_RETURN(handle, returnValue)                                                                                  \
    if (!handle)                                                                                                                  \
    {                                                                                                                             \
        if (!env->ExceptionCheck())                                                                                               \
        {                                                                                                                         \
            env->ThrowNew(env->FindClass("java/lang/IllegalArgumentException"), "Invalid object handle (" STRINGIFY(handle) ")"); \
        }                                                                                                                         \
        return returnValue;                                                                                                       \
    }

#define CHECK_HANDLE(handle)                                                                                               \
    if (!handle)                                                                                                                  \
    {                                                                                                                             \
        if (!env->ExceptionCheck())                                                                                               \
        {                                                                                                                         \
            env->ThrowNew(env->FindClass("java/lang/IllegalArgumentException"), "Invalid object handle (" STRINGIFY(handle) ")"); \
        }                                                                                                                         \
        return;                                                                                                                   \
    }