#include "../include/alphaskia.h"
#include "../../externals/skia/include/core/SkData.h"

extern "C"
{
    AS_API alphaskia_data_t alphaskia_data_new_copy(const uint8_t *data, uint64_t length)
    {
        if ((!data && length > 0) || length > SIZE_MAX)
        {
            return nullptr;
        }

        sk_sp<SkData> skData = SkData::MakeWithCopy(data, static_cast<size_t>(length));
        return reinterpret_cast<alphaskia_data_t>(new sk_sp<SkData>(skData));
    }

    AS_API const uint8_t *alphaskia_data_get_data(alphaskia_data_t data)
    {
        if (!data)
        {
            return nullptr;
        }

        sk_sp<SkData> *internal = reinterpret_cast<sk_sp<SkData> *>(data);
        return (*internal)->bytes();
    }

    AS_API uint64_t alphaskia_data_get_length(alphaskia_data_t data)
    {
        if (!data)
        {
            return 0;
        }

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
