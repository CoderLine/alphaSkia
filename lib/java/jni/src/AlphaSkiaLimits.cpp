#include "../include/alphaTab_alphaSkia_AlphaSkiaLimits.h"
#include "../../../../wrapper/include/alphaskia.h"

extern "C"
{
    JNIEXPORT jlong JNICALL Java_alphaTab_alphaSkia_AlphaSkiaLimits_getLimit(JNIEnv *, jclass, jint limit)
    {
        return static_cast<jlong>(alphaskia_get_limit(static_cast<alphaskia_limit_t>(limit)));
    }

    JNIEXPORT void JNICALL Java_alphaTab_alphaSkia_AlphaSkiaLimits_setLimit(JNIEnv *, jclass, jint limit, jlong value)
    {
        alphaskia_set_limit(static_cast<alphaskia_limit_t>(limit), static_cast<uint64_t>(value));
    }
}
