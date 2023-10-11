#include "../include/alphaskia.h"
#include "../../externals/skia/include/core/SkTypeface.h"
#include "../../externals/skia/include/core/SkData.h"

extern "C"
{
    AS_API alphaskia_typeface_t alphaskia_typeface_register(alphaskia_data_t data)
    {
        sk_sp<SkData> *skData = reinterpret_cast<sk_sp<SkData> *>(data);
        sk_sp<SkTypeface> skTypeFace = SkTypeface::MakeFromData(*skData, 0);
        if (!skTypeFace)
        {
            return nullptr;
        }

        return reinterpret_cast<alphaskia_typeface_t>(new sk_sp<SkTypeface>(skTypeFace));
    }

    AS_API void alphaskia_typeface_free(alphaskia_typeface_t type_face)
    {
        if (!type_face)
        {
            return;
        }

        sk_sp<SkTypeface> *skTypeFace = reinterpret_cast<sk_sp<SkTypeface> *>(type_face);
        delete skTypeFace;
    }

    AS_API alphaskia_typeface_t alphaskia_typeface_make_from_name(const char *name, uint8_t bold, uint8_t italic)
    {
        SkFontStyle style(bold ? SkFontStyle::kBold_Weight : SkFontStyle::kNormal_Weight,
                          SkFontStyle::kNormal_Width,
                          italic ? SkFontStyle::kItalic_Slant : SkFontStyle::kUpright_Slant);
        sk_sp<SkTypeface> skTypeFace = SkTypeface::MakeFromName(name, style);
        
        if (!skTypeFace)
        {
            return nullptr;
        }

        return reinterpret_cast<alphaskia_typeface_t>(new sk_sp<SkTypeface>(skTypeFace));
    }
}