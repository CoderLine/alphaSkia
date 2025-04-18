#pragma once

#include "../../externals/skia/include/core/SkFontMgr.h"
#include "../../externals/skia/src/core/SkTHash.h"

#include <set>
#include <vector>

class SkFontStyleSet_Custom;

class SK_API SkFontMgr_AlphaSkia : public SkFontMgr
{
public:
    explicit SkFontMgr_AlphaSkia();

    void switch_to_operating_system_fonts();
    void switch_to_freetype_fonts();

    static sk_sp<SkFontMgr_AlphaSkia> instance();

protected:
    int onCountFamilies() const override;
    void onGetFamilyName(int index, SkString *familyName) const override;
    sk_sp<SkFontStyleSet> onCreateStyleSet(int index) const override;
    sk_sp<SkFontStyleSet> onMatchFamily(const char familyName[]) const override;
    sk_sp<SkTypeface> onMatchFamilyStyle(const char familyName[],
                                         const SkFontStyle &fontStyle) const override;
    sk_sp<SkTypeface> onMatchFamilyStyleCharacter(const char familyName[], const SkFontStyle &,
                                                  const char *bcp47[], int bcp47Count,
                                                  SkUnichar character) const override;
    sk_sp<SkTypeface> onMakeFromData(sk_sp<SkData> data, int ttcIndex) const override;
    sk_sp<SkTypeface> onMakeFromStreamIndex(std::unique_ptr<SkStreamAsset>, int ttcIndex) const override;
    sk_sp<SkTypeface> onMakeFromStreamArgs(std::unique_ptr<SkStreamAsset>, const SkFontArguments &) const override;
    sk_sp<SkTypeface> onMakeFromFile(const char path[], int ttcIndex) const override;
    sk_sp<SkTypeface> onLegacyMakeTypeface(const char familyName[], SkFontStyle style) const override;


private:
    void registerTypeface(const sk_sp<SkTypeface> &typeface) const;

    sk_sp<SkFontMgr> currentFontMgr_;
    skia_private::THashMap<SkString, sk_sp<SkFontStyleSet_Custom>> *currentFontMgrFontStyleSets_;

    sk_sp<SkFontMgr> freeTypeFontMgr_;
    skia_private::THashMap<SkString, sk_sp<SkFontStyleSet_Custom>> freeTypeFontStyleSets_;

    sk_sp<SkFontMgr> operatingSystemTypeFontMgr_;
    skia_private::THashMap<SkString, sk_sp<SkFontStyleSet_Custom>> operatingSystemFontStyleSets_;
};
