#include "../include/alphaTab_alphaSkia_AlphaSkiaTypeface.h"
#include "../include/JniHelper.h"
#include "../../../../wrapper/include/alphaskia.h"

#include <cstdint>
#include <string>

extern "C"
{

    JNIEXPORT void JNICALL Java_alphaTab_alphaSkia_AlphaSkiaTypeface_release(JNIEnv *, jclass, jlong handle)
    {
        alphaskia_typeface_free(reinterpret_cast<alphaskia_typeface_t>(handle));
    }

    JNIEXPORT jlong JNICALL Java_alphaTab_alphaSkia_AlphaSkiaTypeface_register(JNIEnv *, jclass, jlong data)
    {
        return static_cast<jlong>(reinterpret_cast<std::uintptr_t>(alphaskia_typeface_register(reinterpret_cast<alphaskia_data_t>(data))));
    }

    JNIEXPORT jlong JNICALL Java_alphaTab_alphaSkia_AlphaSkiaTypeface_makeFromName(JNIEnv *env, jclass, jbyteArray utf8Name, jint weight, jboolean italic)
    {
        const std::string name = utf8_from_bytes(env, utf8Name);
        alphaskia_typeface_t typeface = alphaskia_typeface_make_from_name(name.c_str(), static_cast<uint16_t>(weight), italic);
        return static_cast<jlong>(reinterpret_cast<std::uintptr_t>(typeface));
    }

    JNIEXPORT jint JNICALL Java_alphaTab_alphaSkia_AlphaSkiaTypeface_getWeight(JNIEnv *env, jobject instance)
    {
        jlong handle = get_handle(env, instance);
        CHECK_HANDLE_RETURN(handle, static_cast<jint>(400))

        uint16_t weight = alphaskia_typeface_get_weight(reinterpret_cast<alphaskia_typeface_t>(handle));
        return static_cast<jint>(weight);
    }

    JNIEXPORT jboolean JNICALL Java_alphaTab_alphaSkia_AlphaSkiaTypeface_isItalic(JNIEnv *env, jobject instance)
    {
        jlong handle = get_handle(env, instance);
        CHECK_HANDLE_RETURN(handle, static_cast<jboolean>(false))

        uint8_t is_italic = alphaskia_typeface_is_italic(reinterpret_cast<alphaskia_typeface_t>(handle));
        return static_cast<jboolean>(is_italic != 0);
    }

    JNIEXPORT jbyteArray JNICALL Java_alphaTab_alphaSkia_AlphaSkiaTypeface_loadFamilyName(JNIEnv *env, jclass, jlong handle)
    {
        CHECK_HANDLE_RETURN(handle, nullptr)

        alphaskia_string_t family_name = alphaskia_typeface_get_family_name(reinterpret_cast<alphaskia_typeface_t>(handle));
        jbyteArray utf8 = bytes_from_utf8(env, alphaskia_string_get_utf8(family_name), static_cast<size_t>(alphaskia_string_get_length(family_name)));
        alphaskia_string_free(family_name);
        return utf8;
    }
}