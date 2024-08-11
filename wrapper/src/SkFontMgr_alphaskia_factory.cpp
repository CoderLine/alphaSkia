#include "../../externals/skia/include/core/SkFontMgr.h"
#include "../../externals/skia/include/ports/SkTypeface_win.h"

sk_sp<SkFontMgr> SkFontMgr::Factory() {
    // TODO: functions for dynamic changing
    return SkFontMgr_New_DirectWrite();
}
