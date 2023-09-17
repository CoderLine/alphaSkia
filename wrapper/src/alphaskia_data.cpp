#include "../include/libAlphaSkia.h"

#include "../../externals/skia/include/core/SkData.h"

extern "C"
{
    AS_API alphaskia_data_t alphaskia_data_new_copy(const uint8_t *data, uint64_t length)
    {
        sk_sp<SkData> skData = SkData::MakeWithCopy(data, length);
        return reinterpret_cast<alphaskia_data_t>(new sk_sp<SkData>(skData));

    }

    AS_API uint8_t * alphaskia_data_get_data(alphaskia_data_t data)
    {
        sk_sp<SkData> *internal = reinterpret_cast<sk_sp<SkData> *>(data);
        return reinterpret_cast<uint8_t*>((*internal)->writable_data());
    }

    AS_API uint64_t alphaskia_data_get_length(alphaskia_data_t data)
    {
        sk_sp<SkData> *internal = reinterpret_cast<sk_sp<SkData> *>(data);
        return static_cast<uint64_t>((*internal)->size());
    }

    AS_API void alphaskia_data_free(alphaskia_data_t data)
    {
        if (!data)
        {
            return;
        }

        sk_sp<SkData> *internal = reinterpret_cast<sk_sp<SkData> *>(data);
        delete internal;
    }
}