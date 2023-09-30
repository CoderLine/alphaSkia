#include "../include/net_alphatab_alphaskia_AlphaSkiaTypeface.h"
#include "../../../../dist/include/libAlphaSkia.h"

extern "C"
{

    JNIEXPORT void JNICALL Java_net_alphatab_alphaskia_AlphaSkiaTypeface_release(JNIEnv *, jobject, jlong handle)
    {
        alphaskia_typeface_free(reinterpret_cast<alphaskia_typeface_t>(handle));
    }

    JNIEXPORT jlong JNICALL Java_net_alphatab_alphaskia_AlphaSkiaTypeface_register(JNIEnv *, jclass, jlong data)
    {
        return static_cast<jlong>(reinterpret_cast<std::uintptr_t>(alphaskia_typeface_register(reinterpret_cast<alphaskia_data_t>(data))));
    }

    JNIEXPORT jlong JNICALL Java_net_alphatab_alphaskia_AlphaSkiaTypeface_makeFromName(JNIEnv *env, jclass, jstring name, jboolean bold, jboolean italic)
    {
        const char *nativeName = env->GetStringUTFChars(name, nullptr);
        alphaskia_typeface_t typeface = alphaskia_typeface_make_from_name(nativeName, bold, italic);
        env->ReleaseStringUTFChars(name, nativeName);
        return static_cast<jlong>(reinterpret_cast<std::uintptr_t>(typeface));
    }
}