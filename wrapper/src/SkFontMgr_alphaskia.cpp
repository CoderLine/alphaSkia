#include "../include/SkFontMgr_alphaskia.h"

#include "../../externals/skia/include/core/SkSpan.h"
#include "../../externals/skia/include/core/SkFontStyle.h"
#include "../../externals/skia/include/core/SkTypeface.h"
#include "../../externals/skia/include/core/SkStream.h"
#include "../../externals/skia/include/ports/SkFontMgr_data.h"

#if defined(ALPHASKIA_FONTMGR_WINDOWS)
#include "../../externals/skia/include/ports/SkTypeface_win.h"
#define CREATE_OPERATING_SYSTEM_FONTMGR SkFontMgr_New_DirectWrite()
#elif defined(ALPHASKIA_FONTMGR_LINUX)
#include "../../externals/skia/include/ports/SkFontMgr_fontconfig.h"
#define CREATE_OPERATING_SYSTEM_FONTMGR SkFontMgr_New_FontConfig(nullptr)
#elif defined(ALPHASKIA_FONTMGR_ANDROID)
#include "../../externals/skia/include/ports/SkFontMgr_android.h"
#define CREATE_OPERATING_SYSTEM_FONTMGR SkFontMgr_New_Android(nullptr)
#elif defined(ALPHASKIA_FONTMGR_MAC)
#include "../../externals/skia/include/ports/SkFontMgr_mac_ct.h"
#define CREATE_OPERATING_SYSTEM_FONTMGR SkFontMgr_New_CoreText(nullptr)
#elif defined(ALPHASKIA_FONTMGR_IOS)
#include "../../externals/skia/include/ports/SkFontMgr_mac_ct.h"
#define CREATE_OPERATING_SYSTEM_FONTMGR SkFontMgr_New_CoreText(nullptr)
#else
#error "Unsupported operating system - extend font mgr"
#endif

SkFontMgr_AlphaSkia::SkFontMgr_AlphaSkia()
{
    operatingSystemTypeFontMgr_ = CREATE_OPERATING_SYSTEM_FONTMGR;
    currentFontMgr_ = operatingSystemTypeFontMgr_;
}

void SkFontMgr_AlphaSkia::switch_to_operating_system_fonts()
{
    currentFontMgr_ = operatingSystemTypeFontMgr_;
}

void SkFontMgr_AlphaSkia::switch_to_freetype_fonts()
{
    if (!freeTypeFontMgr_)
    {
        freeTypeFontMgr_ = SkFontMgr_New_Custom_Data(SkSpan<sk_sp<SkData>>(nullptr, 0));
    }
    currentFontMgr_ = freeTypeFontMgr_;
}

int SkFontMgr_AlphaSkia::onCountFamilies() const
{
    return currentFontMgr_->countFamilies();
}

void SkFontMgr_AlphaSkia::onGetFamilyName(int index, SkString *familyName) const
{
    currentFontMgr_->getFamilyName(index, familyName);
}
sk_sp<SkFontStyleSet> SkFontMgr_AlphaSkia::onCreateStyleSet(int index) const
{
    return currentFontMgr_->createStyleSet(index);
}

sk_sp<SkFontStyleSet> SkFontMgr_AlphaSkia::onMatchFamily(const char familyName[]) const
{
    return currentFontMgr_->matchFamily(familyName);
}

sk_sp<SkTypeface> SkFontMgr_AlphaSkia::onMatchFamilyStyle(const char familyName[],
                                                        const SkFontStyle &fontStyle) const
{
    return currentFontMgr_->matchFamilyStyle(familyName, fontStyle);
}

sk_sp<SkTypeface> SkFontMgr_AlphaSkia::onMatchFamilyStyleCharacter(const char familyName[], const SkFontStyle &style,
                                                                 const char *bcp47[], int bcp47Count,
                                                                 SkUnichar character) const
{
    return currentFontMgr_->matchFamilyStyleCharacter(familyName, style, bcp47, bcp47Count, character);
}

sk_sp<SkTypeface> SkFontMgr_AlphaSkia::onMakeFromData(sk_sp<SkData> data, int ttcIndex) const
{
    return currentFontMgr_->makeFromData(data, ttcIndex);
}

sk_sp<SkTypeface> SkFontMgr_AlphaSkia::onMakeFromStreamIndex(std::unique_ptr<SkStreamAsset> stream, int ttcIndex) const
{
    if (stream == nullptr)
    {
        return nullptr;
    }
    return currentFontMgr_->makeFromStream(std::move(stream), ttcIndex);
}

sk_sp<SkTypeface> SkFontMgr_AlphaSkia::onMakeFromStreamArgs(std::unique_ptr<SkStreamAsset> stream, const SkFontArguments &args) const
{
    if (stream == nullptr)
    {
        return nullptr;
    }
    return currentFontMgr_->makeFromStream(std::move(stream), args);
}

sk_sp<SkTypeface> SkFontMgr_AlphaSkia::onMakeFromFile(const char path[], int ttcIndex) const
{
    return currentFontMgr_->makeFromFile(path, ttcIndex);
}

sk_sp<SkTypeface> SkFontMgr_AlphaSkia::onLegacyMakeTypeface(const char familyName[], SkFontStyle style) const
{
    return currentFontMgr_->legacyMakeTypeface(familyName, style);
}