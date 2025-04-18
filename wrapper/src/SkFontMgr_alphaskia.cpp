#include "../include/SkFontMgr_alphaskia.h"

#include "../../externals/skia/include/core/SkSpan.h"
#include "../../externals/skia/include/core/SkFontStyle.h"
#include "../../externals/skia/include/core/SkTypeface.h"
#include "../../externals/skia/include/core/SkStream.h"
#include "../../externals/skia/include/ports/SkFontMgr_data.h"
#include "../../externals/skia/src/ports/SkFontMgr_custom.h"

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

sk_sp<SkFontMgr_AlphaSkia> SkFontMgr_AlphaSkia::instance()
{
    static SkOnce once;
    static sk_sp<SkFontMgr_AlphaSkia> singleton;

    once([]
         { singleton = std::move(sk_make_sp<SkFontMgr_AlphaSkia>()); });
    return singleton;
}

SkFontMgr_AlphaSkia::SkFontMgr_AlphaSkia()
{
    operatingSystemTypeFontMgr_ = CREATE_OPERATING_SYSTEM_FONTMGR;
    currentFontMgr_ = operatingSystemTypeFontMgr_;
    currentFontMgrFontStyleSets_ = &operatingSystemFontStyleSets_;
}

void SkFontMgr_AlphaSkia::switch_to_operating_system_fonts()
{
    currentFontMgr_ = operatingSystemTypeFontMgr_;
    currentFontMgrFontStyleSets_ = &operatingSystemFontStyleSets_;
}

void SkFontMgr_AlphaSkia::switch_to_freetype_fonts()
{
    if (!freeTypeFontMgr_)
    {
        freeTypeFontMgr_ = SkFontMgr_New_Custom_Data(SkSpan<sk_sp<SkData>>(nullptr, 0));
    }
    currentFontMgr_ = freeTypeFontMgr_;
    currentFontMgrFontStyleSets_ = &freeTypeFontStyleSets_;
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
    // custom matching with loaded fonts (underlying manager sometimes doesn't lookup newly loaded fonts, skia isn't that dynamic)
    auto it = currentFontMgrFontStyleSets_->find(SkString(familyName));
    if (it)
    {
        return *it;
    }

    auto styleSet = currentFontMgr_->matchFamily(familyName);
    if (styleSet)
    {
        return styleSet;
    }

    return nullptr;
}

sk_sp<SkTypeface> SkFontMgr_AlphaSkia::onMatchFamilyStyle(const char familyName[],
                                                          const SkFontStyle &fontStyle) const
{
    // custom matching with loaded fonts (underlying manager sometimes doesn't lookup newly loaded fonts, skia isn't that dynamic)
    auto it = currentFontMgrFontStyleSets_->find(SkString(familyName));
    if (it)
    {
        return (*it)->matchStyle(fontStyle);
    }

    auto typeface = currentFontMgr_->matchFamilyStyle(familyName, fontStyle);
    if (typeface)
    {
        return typeface;
    }

    return nullptr;
}

sk_sp<SkTypeface> SkFontMgr_AlphaSkia::onMatchFamilyStyleCharacter(const char familyName[], const SkFontStyle &style,
                                                                   const char *bcp47[], int bcp47Count,
                                                                   SkUnichar character) const
{
    auto typeface = currentFontMgr_->matchFamilyStyleCharacter(familyName, style, bcp47, bcp47Count, character);
    if (typeface)
    {
        return typeface;
    }

    // better than nothing? lets match by style and hope for the best
    return onMatchFamilyStyle(familyName, style);
}

sk_sp<SkTypeface> SkFontMgr_AlphaSkia::onMakeFromData(sk_sp<SkData> data, int ttcIndex) const
{
    auto typeface = currentFontMgr_->makeFromData(data, ttcIndex);
    registerTypeface(typeface);
    return typeface;
}

sk_sp<SkTypeface> SkFontMgr_AlphaSkia::onMakeFromStreamIndex(std::unique_ptr<SkStreamAsset> stream, int ttcIndex) const
{
    if (stream == nullptr)
    {
        return nullptr;
    }
    auto typeface = currentFontMgr_->makeFromStream(std::move(stream), ttcIndex);
    registerTypeface(typeface);
    return typeface;
}

sk_sp<SkTypeface> SkFontMgr_AlphaSkia::onMakeFromStreamArgs(std::unique_ptr<SkStreamAsset> stream, const SkFontArguments &args) const
{
    if (stream == nullptr)
    {
        return nullptr;
    }
    auto typeface = currentFontMgr_->makeFromStream(std::move(stream), args);
    registerTypeface(typeface);
    return typeface;
}

sk_sp<SkTypeface> SkFontMgr_AlphaSkia::onMakeFromFile(const char path[], int ttcIndex) const
{
    auto typeface = currentFontMgr_->makeFromFile(path, ttcIndex);
    registerTypeface(typeface);
    return typeface;
}

sk_sp<SkTypeface> SkFontMgr_AlphaSkia::onLegacyMakeTypeface(const char familyName[], SkFontStyle style) const
{
    auto typeface = currentFontMgr_->legacyMakeTypeface(familyName, style);
    registerTypeface(typeface);
    return typeface;
}

void SkFontMgr_AlphaSkia::registerTypeface(const sk_sp<SkTypeface> &typeface) const
{
    SkString familyName;
    typeface->getFamilyName(&familyName);

    auto it = currentFontMgrFontStyleSets_->find(familyName);
    if (it)
    {
        (*it)->appendTypeface(typeface);
    }
    else
    {
        auto newFamily = sk_sp<SkFontStyleSet_Custom>(new SkFontStyleSet_Custom(familyName));
        newFamily->appendTypeface(typeface);
        currentFontMgrFontStyleSets_->set(familyName, newFamily);
    }
}