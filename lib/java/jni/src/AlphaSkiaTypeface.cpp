#include "../include/alphaTab_alphaSkia_AlphaSkiaTypeface.h"
#include "../include/JniHelper.h"
#include "../../../../wrapper/include/alphaskia.h"

extern "C"
{

    JNIEXPORT void JNICALL Java_alphaTab_alphaSkia_AlphaSkiaTypeface_release(JNIEnv *, jobject, jlong handle)
    {
        SCOPE_LOG()
        alphaskia_typeface_free(reinterpret_cast<alphaskia_typeface_t>(handle));
    }

    JNIEXPORT jlong JNICALL Java_alphaTab_alphaSkia_AlphaSkiaTypeface_register(JNIEnv *, jclass, jlong data)
    {
        SCOPE_LOG()
        return static_cast<jlong>(reinterpret_cast<std::uintptr_t>(alphaskia_typeface_register(reinterpret_cast<alphaskia_data_t>(data))));
    }

    JNIEXPORT jlong JNICALL Java_alphaTab_alphaSkia_AlphaSkiaTypeface_makeFromName(JNIEnv *env, jclass, jstring name, jboolean bold, jboolean italic)
    {
        SCOPE_LOG()
        const char *nativeName = env->GetStringUTFChars(name, nullptr);
        alphaskia_typeface_t typeface = alphaskia_typeface_make_from_name(nativeName, bold, italic);
        env->ReleaseStringUTFChars(name, nativeName);
        return static_cast<jlong>(reinterpret_cast<std::uintptr_t>(typeface));
    }

    JNIEXPORT jboolean JNICALL Java_alphaTab_alphaSkia_AlphaSkiaTypeface_isBold(JNIEnv *env, jobject instance)
    {
        SCOPE_LOG()
        jlong handle = get_handle(env, instance);
        CHECK_HANDLE_RETURN(handle, static_cast<jboolean>(false))

        uint8_t is_bold = alphaskia_typeface_is_bold(reinterpret_cast<alphaskia_typeface_t>(handle));
        return static_cast<jboolean>(is_bold != 0);
    }

    JNIEXPORT jboolean JNICALL Java_alphaTab_alphaSkia_AlphaSkiaTypeface_isItalic(JNIEnv *env, jobject instance)
    {
        SCOPE_LOG()
        jlong handle = get_handle(env, instance);
        CHECK_HANDLE_RETURN(handle, static_cast<jboolean>(false))

        uint8_t is_italic = alphaskia_typeface_is_italic(reinterpret_cast<alphaskia_typeface_t>(handle));
        return static_cast<jboolean>(is_italic != 0);
    }

    JNIEXPORT jstring JNICALL Java_alphaTab_alphaSkia_AlphaSkiaTypeface_loadFamilyName(JNIEnv *env, jclass, jlong handle)
    {
        SCOPE_LOG()
        CHECK_HANDLE_RETURN(handle, nullptr)

        alphaskia_string_t family_name = alphaskia_typeface_get_family_name(reinterpret_cast<alphaskia_typeface_t>(handle));

        const char *family_name_chars = alphaskia_string_get_utf8(family_name);
        jstring java_family_name = env->NewStringUTF(family_name_chars);

        alphaskia_string_free(family_name);

        return java_family_name;
    }
}