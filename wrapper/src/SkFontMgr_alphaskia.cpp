#include "../include/SkFontMgr_alphaskia.h"

#include "../../externals/skia/include/core/SkSpan.h"
#include "../../externals/skia/include/core/SkFontArguments.h"
#include "../../externals/skia/include/core/SkFontStyle.h"
#include "../../externals/skia/include/core/SkGraphics.h"
#include "../../externals/skia/include/core/SkTypeface.h"
#include "../../externals/skia/include/core/SkStream.h"
#include "../../externals/skia/include/ports/SkFontMgr_data.h"
#include "../../externals/skia/src/ports/SkFontMgr_custom.h"

#include <algorithm>

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
#elif defined(ALPHASKIA_FONTMGR_MAC) || defined(ALPHASKIA_FONTMGR_IOS)
#include "../../externals/skia/include/ports/SkFontMgr_mac_ct.h"
#include "../../externals/skia/include/ports/SkTypeface_mac.h"
#include "../../externals/skia/src/utils/mac/SkUniqueCFRef.h"
#include <climits>
#define CREATE_OPERATING_SYSTEM_FONTMGR SkFontMgr_New_CoreText(nullptr)
#define ALPHASKIA_FONTMGR_CORETEXT
#else
#error "Unsupported operating system - extend font mgr"
#endif

namespace
{
#if defined(ALPHASKIA_FONTMGR_CORETEXT)
    // SkTypeface::openStream of CoreText typefaces rebuilds the font in memory and keeps it in the typeface, which
    // stays in an internal cache of Skia. The font file is mapped instead.
    std::unique_ptr<SkStreamAsset> open_font_file(const SkTypeface &systemTypeface, const SkFontMgr &freeTypeFontMgr,
                                                  int *ttcIndex)
    {
        CTFontRef ctFont = SkTypeface_GetCTFontRef(&systemTypeface);
        SkString postScriptName;
        if (!ctFont || !systemTypeface.getPostScriptName(&postScriptName))
        {
            return nullptr;
        }

        SkUniqueCFRef<CFURLRef> url(static_cast<CFURLRef>(CTFontCopyAttribute(ctFont, kCTFontURLAttribute)));
        char path[PATH_MAX];
        if (!url || !CFURLGetFileSystemRepresentation(url.get(), true, reinterpret_cast<UInt8 *>(path), sizeof(path)))
        {
            return nullptr;
        }

        auto stream = SkStream::MakeFromFile(path);
        if (!stream)
        {
            return nullptr;
        }

        // the face of a font collection is identified by its PostScript name, FreeType face indices are 16 bit
        int faceCount = 0;
        for (; faceCount <= 0xFFFF; faceCount++)
        {
            auto face = freeTypeFontMgr.makeFromStream(stream->duplicate(), faceCount);
            if (!face)
            {
                break;
            }

            SkString facePostScriptName;
            if (face->getPostScriptName(&facePostScriptName) && facePostScriptName == postScriptName)
            {
                *ttcIndex = faceCount;
                return stream;
            }
        }

        // instances of a variable font report their own PostScript name
        if (faceCount == 1)
        {
            *ttcIndex = 0;
            return stream;
        }
        return nullptr;
    }
#endif

    std::unique_ptr<SkStreamAsset> open_system_font(const SkTypeface &systemTypeface, const SkFontMgr &freeTypeFontMgr,
                                                    int *ttcIndex)
    {
#if defined(ALPHASKIA_FONTMGR_CORETEXT)
        if (auto stream = open_font_file(systemTypeface, freeTypeFontMgr, ttcIndex))
        {
            return stream;
        }
#endif
        return systemTypeface.openStream(ttcIndex);
    }
}

// Hands out the typefaces of an operating system style set as FreeType typefaces.
class SkFontStyleSet_AlphaSkiaSystem final : public SkFontStyleSet
{
public:
    SkFontStyleSet_AlphaSkiaSystem(sk_sp<const SkFontMgr_AlphaSkia> fontMgr, sk_sp<SkFontStyleSet> systemStyleSet)
        : fontMgr_(std::move(fontMgr)), systemStyleSet_(std::move(systemStyleSet))
    {
    }

    int count() override
    {
        return systemStyleSet_->count();
    }

    void getStyle(int index, SkFontStyle *style, SkString *name) override
    {
        systemStyleSet_->getStyle(index, style, name);
    }

    sk_sp<SkTypeface> createTypeface(int index) override
    {
        return fontMgr_->toFreeTypeTypeface(systemStyleSet_->createTypeface(index));
    }

    sk_sp<SkTypeface> matchStyle(const SkFontStyle &pattern) override
    {
        return fontMgr_->toFreeTypeTypeface(systemStyleSet_->matchStyle(pattern));
    }

private:
    sk_sp<const SkFontMgr_AlphaSkia> fontMgr_;
    sk_sp<SkFontStyleSet> systemStyleSet_;
};

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
    {
        std::lock_guard<std::mutex> lock(mutex_);
        currentFontMgr_ = operatingSystemTypeFontMgr_;
        currentFontMgrFontStyleSets_ = &operatingSystemFontStyleSets_;
    }
    purge_font_caches();
}

void SkFontMgr_AlphaSkia::switch_to_freetype_fonts()
{
    {
        std::lock_guard<std::mutex> lock(mutex_);
        if (!freeTypeFontMgr_)
        {
            freeTypeFontMgr_ = SkFontMgr_New_Custom_Data(SkSpan<sk_sp<SkData>>(nullptr, 0));
        }
        currentFontMgr_ = freeTypeFontMgr_;
        currentFontMgrFontStyleSets_ = &freeTypeFontStyleSets_;
    }
    purge_font_caches();
}

void SkFontMgr_AlphaSkia::purge_font_caches()
{
    SystemTypefaces released;
    {
        std::lock_guard<std::mutex> lock(mutex_);
        released.swap(systemTypefaces_);
    }
    // after clearing, so FontCollections clearing their caches cannot pick up the released typefaces again
    fontGeneration_.fetch_add(1);
    SkGraphics::PurgeFontCache();
}

uint64_t SkFontMgr_AlphaSkia::font_generation() const
{
    return fontGeneration_.load();
}

SkFontMgr_AlphaSkia::Current SkFontMgr_AlphaSkia::current() const
{
    std::lock_guard<std::mutex> lock(mutex_);
    const bool isFreeType = currentFontMgr_ == freeTypeFontMgr_;
    return {currentFontMgr_, currentFontMgrFontStyleSets_, isFreeType ? operatingSystemTypeFontMgr_ : nullptr};
}

sk_sp<SkTypeface> SkFontMgr_AlphaSkia::toFreeTypeTypeface(const sk_sp<SkTypeface> &systemTypeface) const
{
    if (!systemTypeface)
    {
        return nullptr;
    }

    const SkTypefaceID systemTypefaceId = systemTypeface->uniqueID();
    sk_sp<SkFontMgr> freeTypeFontMgr;
    {
        std::lock_guard<std::mutex> lock(mutex_);
        if (auto converted = systemTypefaces_.find(systemTypefaceId))
        {
            return *converted;
        }
        freeTypeFontMgr = freeTypeFontMgr_;
    }

    if (!freeTypeFontMgr)
    {
        return nullptr;
    }

    sk_sp<SkTypeface> converted;
    int ttcIndex = 0;
    if (auto stream = open_system_font(*systemTypeface, *freeTypeFontMgr, &ttcIndex))
    {
        SkFontArguments args;
        args.setCollectionIndex(ttcIndex);

        // variable fonts and named instances keep the position the operating system chose
        std::vector<SkFontArguments::VariationPosition::Coordinate> coordinates(
            std::max(0, systemTypeface->getVariationDesignPosition({})));
        if (!coordinates.empty() &&
            systemTypeface->getVariationDesignPosition(coordinates) == static_cast<int>(coordinates.size()))
        {
            args.setVariationDesignPosition({coordinates.data(), static_cast<int>(coordinates.size())});
        }

        converted = freeTypeFontMgr->makeFromStream(std::move(stream), args);
    }

    // failed conversions are kept too, they would fail again
    std::lock_guard<std::mutex> lock(mutex_);
    if (auto concurrentlyConverted = systemTypefaces_.find(systemTypefaceId))
    {
        return *concurrentlyConverted;
    }
    systemTypefaces_.set(systemTypefaceId, converted);
    return converted;
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

    auto styleSet = current.fontMgr->matchFamily(familyName);
    if (!current.systemFontMgr || styleSet->count() > 0)
    {
        return styleSet;
    }

    auto systemStyleSet = current.systemFontMgr->matchFamily(familyName);
    if (systemStyleSet->count() == 0)
    {
        return systemStyleSet;
    }
    return sk_make_sp<SkFontStyleSet_AlphaSkiaSystem>(sk_ref_sp(this), std::move(systemStyleSet));
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

    auto typeface = current.fontMgr->matchFamilyStyle(familyName, fontStyle);
    if (typeface || !current.systemFontMgr)
    {
        return typeface;
    }
    return toFreeTypeTypeface(current.systemFontMgr->matchFamilyStyle(familyName, fontStyle));
}

sk_sp<SkTypeface> SkFontMgr_AlphaSkia::onMatchFamilyStyleCharacter(const char familyName[], const SkFontStyle &style,
                                                                   const char *bcp47[], int bcp47Count,
                                                                   SkUnichar character) const
{
    const Current current = this->current();
    auto typeface = current.fontMgr->matchFamilyStyleCharacter(familyName, style, bcp47, bcp47Count, character);
    if (!typeface && current.systemFontMgr)
    {
        typeface = toFreeTypeTypeface(
            current.systemFontMgr->matchFamilyStyleCharacter(familyName, style, bcp47, bcp47Count, character));
    }
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
    const Current current = this->current();

    // the operating system font managers do not find fonts created from data by their family name
    if (familyName)
    {
        if (auto registered = findRegisteredFamily(current, familyName))
        {
            return registered->matchStyle(style);
        }
    }

    if (!current.systemFontMgr)
    {
        return current.fontMgr->legacyMakeTypeface(familyName, style);
    }

    // the operating system also resolves a null or unknown family to its default font
    if (auto typeface = toFreeTypeTypeface(current.systemFontMgr->legacyMakeTypeface(familyName, style)))
    {
        return typeface;
    }
    return current.fontMgr->legacyMakeTypeface(familyName, style);
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
