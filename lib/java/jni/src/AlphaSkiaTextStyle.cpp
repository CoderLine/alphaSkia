#include "../include/alphaTab_alphaSkia_AlphaSkiaTypeface.h"
#include "../include/JniHelper.h"
#include "../../../../wrapper/include/alphaskia.h"
#include <vector>

extern "C"
{
    JNIEXPORT jlong JNICALL Java_alphaTab_alphaSkia_AlphaSkiaTextStyle_alphaskiaTextStyleNew(JNIEnv *env, jclass, jobjectArray familyNames, jint weight, jboolean isItalic)
    {
        uint8_t familyNameLength = static_cast<uint8_t>(env->GetArrayLength(familyNames));
        std::vector<const char *> nativeFamilyNames;
        nativeFamilyNames.resize(familyNameLength);

        for (uint8_t i = 0; i < familyNameLength; i++)
        {
            jstring familyName = static_cast<jstring>(env->GetObjectArrayElement(familyNames, i));
            nativeFamilyNames[i] = env->GetStringUTFChars(familyName, nullptr);
        }

        alphaskia_text_style_t canvas = alphaskia_text_style_new(
            familyNameLength,
            &nativeFamilyNames[0],
            static_cast<uint16_t>(weight),
            isItalic ? 1 : 0);

        for (uint8_t i = 0; i < familyNameLength; i++)
        {
            jstring familyName = static_cast<jstring>(env->GetObjectArrayElement(familyNames, i));
            env->ReleaseStringUTFChars(familyName, nativeFamilyNames[i]);
        }
        return static_cast<jlong>(reinterpret_cast<std::uintptr_t>(canvas));
    }

    JNIEXPORT void JNICALL Java_alphaTab_alphaSkia_AlphaSkiaTextStyle_close(JNIEnv *env, jobject instance)
    {
        alphaskia_text_style_t textStyle = reinterpret_cast<alphaskia_text_style_t>(get_handle(env, instance));
        CHECK_HANDLE(textStyle)

        alphaskia_text_style_free(textStyle);
        set_handle(env, instance, 0);
    }
}