#include <jni.h>
#include <iostream>

#define STRINGIFY(s) _STRINGIFY(s)
#define _STRINGIFY(s) #s

class ScopeLog
{
public:
    ScopeLog(const char *function_name) : function_name_(function_name)
    {
        std::cout << "Enter " << function_name << std::endl;
    }
    ~ScopeLog() {
        std::cout << "Exit " << function_name_ << std::endl;
    }
private:
    const char *function_name_;
};

#define SCOPE_LOG() ScopeLog scope_log(__FUNCTION__);

inline jlong
get_handle(JNIEnv *env, jobject instance)
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

#define CHECK_HANDLE(handle)                                                                                                      \
    if (!handle)                                                                                                                  \
    {                                                                                                                             \
        if (!env->ExceptionCheck())                                                                                               \
        {                                                                                                                         \
            env->ThrowNew(env->FindClass("java/lang/IllegalArgumentException"), "Invalid object handle (" STRINGIFY(handle) ")"); \
        }                                                                                                                         \
        return;                                                                                                                   \
    }