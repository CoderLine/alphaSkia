#include "../include/SkFontMgr_alphaskia.h"

sk_sp<SkFontMgr> SkFontMgr::Factory()
{
    return sk_make_sp<SkFontMgr_AlphaSkia>();
}
