#include <jni.h>

#define STRINGIFY(s) _STRINGIFY(s)
#define _STRINGIFY(s) #s

inline jlong get_handle(JNIEnv *env, jobject instance)
{
    jclass cls = env->FindClass("alphaTab/alphaSkia/AlphaSkiaNative");
    if (!cls)
    {
        env->ThrowNew(env->FindClass("java/lang/IllegalStateException"), "Could not find class alphaTab.alphaSkia.AlphaSkiaNative");
        return 0;
    }
    jfieldID handleFieldId = env->GetFieldID(cls, "handle", "J");
    if (!handleFieldId)
    {
        env->ThrowNew(env->FindClass("java/lang/IllegalStateException"), "Could not find field handle in class alphaTab.alphaSkia.AlphaSkiaNative");
        return 0;
    }
    return env->GetLongField(instance, handleFieldId);
}
inline void set_handle(JNIEnv *env, jobject instance, jlong handle)
{
    jclass cls = env->FindClass("alphaTab/alphaSkia/AlphaSkiaNative");
    jfieldID handleFieldId = env->GetFieldID(cls, "handle", "J");
    env->SetLongField(instance, handleFieldId, handle);
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