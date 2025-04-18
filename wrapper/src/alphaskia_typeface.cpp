#include "../include/alphaskia.h"
#include "../include/SkFontMgr_alphaskia.h"
#include "../../externals/skia/include/core/SkTypeface.h"
#include "../../externals/skia/include/core/SkData.h"
#include "../../externals/skia/include/core/SkFontMgr.h"

extern "C"
{
    AS_API alphaskia_typeface_t alphaskia_typeface_register(alphaskia_data_t data)
    {
        sk_sp<SkData> *skData = reinterpret_cast<sk_sp<SkData> *>(data);
        auto fontMgr = SkFontMgr_AlphaSkia::instance();
        sk_sp<SkTypeface> skTypeface = fontMgr->makeFromData(*skData, 0);
        if (!skTypeface)
        {
            return nullptr;
        }

        return reinterpret_cast<alphaskia_typeface_t>(new sk_sp<SkTypeface>(skTypeface));
    }

    AS_API void alphaskia_typeface_free(alphaskia_typeface_t typeface)
    {
        if (!typeface)
        {
            return;
        }

        sk_sp<SkTypeface> *skTypeface = reinterpret_cast<sk_sp<SkTypeface> *>(typeface);
        delete skTypeface;
    }

    AS_API alphaskia_typeface_t alphaskia_typeface_make_from_name(const char *family_name, uint8_t bold, uint8_t italic)
    {
        auto fontMgr = SkFontMgr_AlphaSkia::instance();

        SkFontStyle style(bold ? SkFontStyle::kBold_Weight : SkFontStyle::kNormal_Weight,
                          SkFontStyle::kNormal_Width,
                          italic ? SkFontStyle::kItalic_Slant : SkFontStyle::kUpright_Slant);
        sk_sp<SkTypeface> skTypeface = fontMgr->legacyMakeTypeface(family_name, style);

        if (!skTypeface)
        {
            return nullptr;
        }

        return reinterpret_cast<alphaskia_typeface_t>(new sk_sp<SkTypeface>(skTypeface));
    }

    AS_API alphaskia_string_t alphaskia_typeface_get_family_name(alphaskia_typeface_t typeface)
    {
        sk_sp<SkTypeface> *skTypeface = reinterpret_cast<sk_sp<SkTypeface> *>(typeface);

        SkString *skFamilyName = new SkString();
        (*skTypeface)->getFamilyName(skFamilyName);

        return reinterpret_cast<alphaskia_string_t>(skFamilyName);
    }

    AS_API uint16_t alphaskia_typeface_get_weigth(alphaskia_typeface_t typeface)
    {
        sk_sp<SkTypeface> *skTypeface = reinterpret_cast<sk_sp<SkTypeface> *>(typeface);
        return static_cast<uint16_t>((*skTypeface)->fontStyle().weight());
    }

    AS_API uint8_t alphaskia_typeface_is_italic(alphaskia_typeface_t typeface)
    {
        sk_sp<SkTypeface> *skTypeface = reinterpret_cast<sk_sp<SkTypeface> *>(typeface);
        return (*skTypeface)->isItalic() ? 1 : 0;
    }
}