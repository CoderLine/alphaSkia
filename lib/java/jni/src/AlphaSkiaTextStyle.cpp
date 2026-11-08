#include "../include/alphaTab_alphaSkia_AlphaSkiaTextStyle.h"
#include "../include/JniHelper.h"
#include "../../../../wrapper/include/alphaskia.h"
#include <string>
#include <vector>

extern "C"
{
    JNIEXPORT jlong JNICALL Java_alphaTab_alphaSkia_AlphaSkiaTextStyle_alphaskiaTextStyleNew(JNIEnv *env, jclass, jobjectArray utf8FamilyNames, jint weight, jboolean isItalic)
    {
        const jsize familyNameCount = env->GetArrayLength(utf8FamilyNames);
        if (familyNameCount > UINT8_MAX)
        {
            env->ThrowNew(env->FindClass("java/lang/IllegalArgumentException"), "At most 255 font families are supported");
            return 0;
        }

        std::vector<std::string> familyNames;
        familyNames.reserve(static_cast<size_t>(familyNameCount));
        for (jsize i = 0; i < familyNameCount; i++)
        {
            jbyteArray familyName = static_cast<jbyteArray>(env->GetObjectArrayElement(utf8FamilyNames, i));
            if (!familyName)
            {
                env->ThrowNew(env->FindClass("java/lang/NullPointerException"), "font family is null");
                return 0;
            }
            familyNames.push_back(utf8_from_bytes(env, familyName));
            env->DeleteLocalRef(familyName);
        }

        std::vector<const char *> nativeFamilyNames;
        nativeFamilyNames.reserve(familyNames.size());
        for (const auto &familyName : familyNames)
        {
            nativeFamilyNames.push_back(familyName.c_str());
        }

        alphaskia_text_style_t textStyle = alphaskia_text_style_new(
            static_cast<uint8_t>(familyNameCount),
            nativeFamilyNames.data(),
            static_cast<uint16_t>(weight),
            isItalic ? 1 : 0);
        return static_cast<jlong>(reinterpret_cast<std::uintptr_t>(textStyle));
    }

    JNIEXPORT void JNICALL Java_alphaTab_alphaSkia_AlphaSkiaTextStyle_release(JNIEnv *, jclass, jlong handle)
    {
        alphaskia_text_style_free(reinterpret_cast<alphaskia_text_style_t>(handle));
    }
}