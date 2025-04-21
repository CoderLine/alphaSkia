#include "../include/alphaskia.h"
#include "../include/AlphaSkiaCanvas.h"

extern "C"
{
    AS_API alphaskia_textstyle_t alphaskia_textstyle_new(uint8_t family_name_count, const char **family_names, uint16_t weight, uint8_t italic)
    {
        SkFontStyle::Weight skWeight;
        switch (weight)
        {
        case SkFontStyle::Weight::kInvisible_Weight:
        case SkFontStyle::Weight::kThin_Weight:
        case SkFontStyle::Weight::kExtraLight_Weight:
        case SkFontStyle::Weight::kLight_Weight:
        case SkFontStyle::Weight::kNormal_Weight:
        case SkFontStyle::Weight::kMedium_Weight:
        case SkFontStyle::Weight::kSemiBold_Weight:
        case SkFontStyle::Weight::kBold_Weight:
        case SkFontStyle::Weight::kExtraBold_Weight:
        case SkFontStyle::Weight::kBlack_Weight:
        case SkFontStyle::Weight::kExtraBlack_Weight:
            skWeight = static_cast<SkFontStyle::Weight>(weight);
            break;
        default:
            skWeight = SkFontStyle::Weight::kNormal_Weight;
            break;
        }

        AlphaSkiaTextStyle *skTextStyle(new AlphaSkiaTextStyle(
            skWeight,
            italic ? SkFontStyle::Slant::kItalic_Slant : SkFontStyle::Slant::kUpright_Slant,
            family_name_count));

        for (int i = 0; i < family_name_count; i++)
        {
            skTextStyle->get_family_names().emplace_back(SkString(family_names[i]));
        }

        return reinterpret_cast<alphaskia_textstyle_t>(skTextStyle);
    }

    AS_API uint8_t alphaskia_textstyle_get_family_name_count(alphaskia_textstyle_t textstyle)
    {
        AlphaSkiaTextStyle *skTextstyle = reinterpret_cast<AlphaSkiaTextStyle *>(textstyle);
        return static_cast<uint8_t>(skTextstyle->get_family_names().size());
    }

    AS_API alphaskia_string_t alphaskia_textstyle_get_family_name(alphaskia_textstyle_t textstyle, uint8_t index)
    {
        AlphaSkiaTextStyle *skTextstyle = reinterpret_cast<AlphaSkiaTextStyle *>(textstyle);

        if (index >= skTextstyle->get_family_names().size())
        {
            return nullptr;
        }

        SkString *skFamilyName = new SkString(skTextstyle->get_family_names()[index]);
        return reinterpret_cast<alphaskia_string_t>(skFamilyName);
    }

    AS_API uint16_t alphaskia_textstyle_get_weight(alphaskia_textstyle_t textstyle)
    {
        AlphaSkiaTextStyle *skTextstyle = reinterpret_cast<AlphaSkiaTextStyle *>(textstyle);
        return static_cast<uint16_t>(skTextstyle->get_font_style().weight());
    }

    AS_API uint8_t alphaskia_textstyle_is_italic(alphaskia_textstyle_t textstyle)
    {
        AlphaSkiaTextStyle *skTextstyle = reinterpret_cast<AlphaSkiaTextStyle *>(textstyle);
        return skTextstyle->get_font_style().slant() == SkFontStyle::Slant::kItalic_Slant ? 1 : 0;
    }

    AS_API void alphaskia_textstyle_free(alphaskia_textstyle_t textstyle)
    {
        if (!textstyle)
        {
            return;
        }

        AlphaSkiaTextStyle *skTextStyle = reinterpret_cast<AlphaSkiaTextStyle *>(textstyle);
        delete skTextStyle;
    }
}