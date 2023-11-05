#include <jni.h>

inline jlong get_handle(JNIEnv *env, jobject instance)
{
    jclass cls = env->FindClass("alphaTab/alphaSkia/AlphaSkiaNative");
    jfieldID handleFieldId = env->GetFieldID(cls, "handle", "J");
    return env->GetLongField(instance, handleFieldId);
}
inline void set_handle(JNIEnv *env, jobject instance, jlong handle)
{
    jclass cls = env->FindClass("alphaTab/alphaSkia/AlphaSkiaNative");
    jfieldID handleFieldId = env->GetFieldID(cls, "handle", "J");
    env->SetLongField(instance, handleFieldId, handle);
}
