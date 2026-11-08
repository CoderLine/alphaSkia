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
#include "../../externals/skia/include/ports/SkFontScanner_FreeType.h"
#define CREATE_OPERATING_SYSTEM_FONTMGR SkFontMgr_New_FontConfig(nullptr, SkFontScanner_Make_FreeType())
#elif defined(ALPHASKIA_FONTMGR_ANDROID)
#include "../../externals/skia/include/ports/SkFontMgr_android.h"
#include "../../externals/skia/include/ports/SkFontScanner_FreeType.h"
#define CREATE_OPERATING_SYSTEM_FONTMGR SkFontMgr_New_Android(nullptr, SkFontScanner_Make_FreeType())
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
    std::lock_guard<std::mutex> lock(mutex_);
    currentFontMgr_ = operatingSystemTypeFontMgr_;
    currentFontMgrFontStyleSets_ = &operatingSystemFontStyleSets_;
}

void SkFontMgr_AlphaSkia::switch_to_freetype_fonts()
{
    std::lock_guard<std::mutex> lock(mutex_);
    if (!freeTypeFontMgr_)
    {
        freeTypeFontMgr_ = SkFontMgr_New_Custom_Data(SkSpan<sk_sp<SkData>>(nullptr, 0));
    }
    currentFontMgr_ = freeTypeFontMgr_;
    currentFontMgrFontStyleSets_ = &freeTypeFontStyleSets_;
}

SkFontMgr_AlphaSkia::Current SkFontMgr_AlphaSkia::current() const
{
    std::lock_guard<std::mutex> lock(mutex_);
    return {currentFontMgr_, currentFontMgrFontStyleSets_};
}

sk_sp<SkFontStyleSet_Custom> SkFontMgr_AlphaSkia::findRegisteredFamily(const Current &current, const char familyName[]) const
{
    std::lock_guard<std::mutex> lock(mutex_);
    auto it = current.styleSets->find(SkString(familyName));
    if (!it)
    {
        return nullptr;
    }

    // a copy, the registered set can grow while the caller uses the returned one
    auto copy = sk_make_sp<SkFontStyleSet_Custom>((*it)->getFamilyName());
    for (int i = 0; i < (*it)->count(); i++)
    {
        copy->appendTypeface((*it)->createTypeface(i));
    }
    return copy;
}

int SkFontMgr_AlphaSkia::onCountFamilies() const
{
    return current().fontMgr->countFamilies();
}

void SkFontMgr_AlphaSkia::onGetFamilyName(int index, SkString *familyName) const
{
    current().fontMgr->getFamilyName(index, familyName);
}

sk_sp<SkFontStyleSet> SkFontMgr_AlphaSkia::onCreateStyleSet(int index) const
{
    return current().fontMgr->createStyleSet(index);
}

sk_sp<SkFontStyleSet> SkFontMgr_AlphaSkia::onMatchFamily(const char familyName[]) const
{
    // custom matching with loaded fonts (underlying manager sometimes doesn't lookup newly loaded fonts, skia isn't that dynamic)
    const Current current = this->current();
    if (auto registered = findRegisteredFamily(current, familyName))
    {
        return registered;
    }

    return current.fontMgr->matchFamily(familyName);
}

sk_sp<SkTypeface> SkFontMgr_AlphaSkia::onMatchFamilyStyle(const char familyName[],
                                                          const SkFontStyle &fontStyle) const
{
    // custom matching with loaded fonts (underlying manager sometimes doesn't lookup newly loaded fonts, skia isn't that dynamic)
    const Current current = this->current();
    if (auto registered = findRegisteredFamily(current, familyName))
    {
        return registered->matchStyle(fontStyle);
    }

    return current.fontMgr->matchFamilyStyle(familyName, fontStyle);
}

sk_sp<SkTypeface> SkFontMgr_AlphaSkia::onMatchFamilyStyleCharacter(const char familyName[], const SkFontStyle &style,
                                                                   const char *bcp47[], int bcp47Count,
                                                                   SkUnichar character) const
{
    auto typeface = current().fontMgr->matchFamilyStyleCharacter(familyName, style, bcp47, bcp47Count, character);
    if (typeface)
    {
        return typeface;
    }

    // better than nothing? lets match by style and hope for the best
    return onMatchFamilyStyle(familyName, style);
}

sk_sp<SkTypeface> SkFontMgr_AlphaSkia::onMakeFromData(sk_sp<SkData> data, int ttcIndex) const
{
    const Current current = this->current();
    auto typeface = current.fontMgr->makeFromData(data, ttcIndex);
    registerTypeface(current, typeface);
    return typeface;
}

sk_sp<SkTypeface> SkFontMgr_AlphaSkia::onMakeFromStreamIndex(std::unique_ptr<SkStreamAsset> stream, int ttcIndex) const
{
    if (stream == nullptr)
    {
        return nullptr;
    }
    const Current current = this->current();
    auto typeface = current.fontMgr->makeFromStream(std::move(stream), ttcIndex);
    registerTypeface(current, typeface);
    return typeface;
}

sk_sp<SkTypeface> SkFontMgr_AlphaSkia::onMakeFromStreamArgs(std::unique_ptr<SkStreamAsset> stream, const SkFontArguments &args) const
{
    if (stream == nullptr)
    {
        return nullptr;
    }
    const Current current = this->current();
    auto typeface = current.fontMgr->makeFromStream(std::move(stream), args);
    registerTypeface(current, typeface);
    return typeface;
}

sk_sp<SkTypeface> SkFontMgr_AlphaSkia::onMakeFromFile(const char path[], int ttcIndex) const
{
    const Current current = this->current();
    auto typeface = current.fontMgr->makeFromFile(path, ttcIndex);
    registerTypeface(current, typeface);
    return typeface;
}

sk_sp<SkTypeface> SkFontMgr_AlphaSkia::onLegacyMakeTypeface(const char familyName[], SkFontStyle style) const
{
    // Typefaces of the underlying manager are found by it again. Registering them would shadow the other styles
    // of their family and grow the registry on every lookup.
    return current().fontMgr->legacyMakeTypeface(familyName, style);
}

void SkFontMgr_AlphaSkia::registerTypeface(const Current &current, const sk_sp<SkTypeface> &typeface) const
{
    if (!typeface)
    {
        return;
    }

    SkString familyName;
    typeface->getFamilyName(&familyName);

    std::lock_guard<std::mutex> lock(mutex_);
    auto it = current.styleSets->find(familyName);
    if (it)
    {
        for (int i = 0; i < (*it)->count(); i++)
        {
            if ((*it)->createTypeface(i) == typeface)
            {
                return;
            }
        }
        (*it)->appendTypeface(typeface);
    }
    else
    {
        auto newFamily = sk_sp<SkFontStyleSet_Custom>(new SkFontStyleSet_Custom(familyName));
        newFamily->appendTypeface(typeface);
        current.styleSets->set(familyName, newFamily);
    }
}
