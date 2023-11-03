#include "../include/alphaskia.h"
#include "../../externals/skia/include/core/SkString.h"

extern "C"
{
    AS_API const char *alphaskia_string_get_utf8(alphaskia_string_t string)
    {
        sk_sp<SkString> *skString = reinterpret_cast<sk_sp<SkString> *>(string);
        return (*skString)->c_str();
    }

    AS_API uint64_t alphaskia_string_get_length(alphaskia_string_t string)
    {
        sk_sp<SkString> *skString = reinterpret_cast<sk_sp<SkString> *>(string);
        return static_cast<uint64_t>((*skString)->size());
    }

    AS_API void alphaskia_string_free(alphaskia_string_t string)
    {
        sk_sp<SkString> *skString = reinterpret_cast<sk_sp<SkString> *>(string);
        delete skString;
    }
}