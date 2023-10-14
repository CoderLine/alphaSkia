#include "../include/alphaskia.h"
#include "../../externals/skia/include/core/SkImage.h"
#include "../../externals/skia/include/core/SkBitmap.h"
#include "../../externals/skia/include/core/SkPixmap.h"
#include "../../externals/skia/include/core/SkStream.h"
#include "../../externals/skia/include/encode/SkPngEncoder.h"
#include "../../externals/skia/include/codec/SkPngDecoder.h"

extern "C"
{
    AS_API void alphaskia_image_free(alphaskia_image_t image)
    {
        sk_sp<SkImage> *internal = reinterpret_cast<sk_sp<SkImage> *>(image);
        delete internal;
    }

    AS_API int32_t alphaskia_image_get_width(alphaskia_image_t image)
    {
        sk_sp<SkImage> *internal = reinterpret_cast<sk_sp<SkImage> *>(image);
        return (*internal)->width();
    }

    AS_API int32_t alphaskia_image_get_height(alphaskia_image_t image)
    {
        sk_sp<SkImage> *internal = reinterpret_cast<sk_sp<SkImage> *>(image);
        return (*internal)->height();
    }

    AS_API uint8_t alphaskia_image_read_pixels(alphaskia_image_t image, uint8_t *pixels, uint64_t rowBytes)
    {
        sk_sp<SkImage> *internal = reinterpret_cast<sk_sp<SkImage> *>(image);

        SkPixmap pixmap((*internal)->imageInfo(), pixels, static_cast<size_t>(rowBytes));
        return (*internal)->readPixels(pixmap, 0, 0) ? 1 : 0;
    }

    AS_API alphaskia_data_t alphaskia_image_encode_png(alphaskia_image_t image)
    {
        sk_sp<SkImage> *internal = reinterpret_cast<sk_sp<SkImage> *>(image);
        SkDynamicMemoryWStream stream;
        SkPngEncoder::Options options;

        sk_sp<SkImage> raster = (*internal)->makeRasterImage();
        SkPixmap pixmap;
        if (!(*internal)->peekPixels(&pixmap))
        {
            return nullptr;
        }
        SkPngEncoder::Encode(static_cast<SkWStream *>(&stream), pixmap, options);
        return reinterpret_cast<alphaskia_data_t>(new sk_sp<SkData>(stream.detachAsData()));
    }

    AS_API alphaskia_image_t alphaskia_image_decode(const uint8_t *data, uint64_t length)
    {
        sk_sp<SkData> wrapped_data = SkData::MakeWithCopy(data, length);
        sk_sp<SkImage> image = SkImages::DeferredFromEncodedData(wrapped_data, SkAlphaType::kPremul_SkAlphaType);
        return reinterpret_cast<alphaskia_image_t>(new sk_sp<SkImage>(image));
    }

    AS_API alphaskia_image_t alphaskia_image_from_pixels(int32_t width, int32_t height, const uint8_t *pixels)
    {
        SkPixmap pixmap(SkImageInfo::Make(width, height, kN32_SkColorType, kPremul_SkAlphaType),
                        pixels, sizeof(uint32_t) * width);
        sk_sp<SkImage> image = SkImages::RasterFromPixmapCopy(pixmap);
        return reinterpret_cast<alphaskia_image_t>(new sk_sp<SkImage>(image));
    }
}