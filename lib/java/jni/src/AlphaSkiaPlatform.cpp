#include "../include/alphaTab_alphaSkia_AlphaSkiaPlatform.h"
#include "../include/JniHelper.h"

bool g_is_trace_enabled = false;

extern "C"
{
    JNIEXPORT void JNICALL Java_alphaTab_alphaSkia_AlphaSkiaPlatform_setTracingEnabled(JNIEnv *, jclass, jboolean is_enabled)
    {
        g_is_trace_enabled = static_cast<bool>(is_enabled);
    }
}