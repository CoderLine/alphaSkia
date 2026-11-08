#include "../include/alphaskia.h"
#include "../../externals/skia/include/core/SkImage.h"
#include "../../externals/skia/include/core/SkBitmap.h"
#include "../../externals/skia/include/core/SkPixmap.h"
#include "../../externals/skia/include/core/SkStream.h"
#include "../../externals/skia/include/encode/SkPngEncoder.h"
#include "../../externals/skia/include/codec/SkPngDecoder.h"

namespace
{
    SkImage *get_image(alphaskia_image_t image)
    {
        sk_sp<SkImage> *internal = reinterpret_cast<sk_sp<SkImage> *>(image);
        return internal ? internal->get() : nullptr;
    }

    alphaskia_image_t wrap_image(sk_sp<SkImage> image)
    {
        if (!image)
        {
            return nullptr;
        }
        return reinterpret_cast<alphaskia_image_t>(new sk_sp<SkImage>(std::move(image)));
    }
}

extern "C"
{
    AS_API void alphaskia_image_free(alphaskia_image_t image)
    {
        sk_sp<SkImage> *internal = reinterpret_cast<sk_sp<SkImage> *>(image);
        delete internal;
    }

    AS_API int32_t alphaskia_image_get_width(alphaskia_image_t image)
    {
        SkImage *internal = get_image(image);
        return internal ? internal->width() : 0;
    }

    AS_API int32_t alphaskia_image_get_height(alphaskia_image_t image)
    {
        SkImage *internal = get_image(image);
        return internal ? internal->height() : 0;
    }

    AS_API uint8_t alphaskia_image_read_pixels(alphaskia_image_t image, uint8_t *pixels, uint64_t row_bytes, uint64_t pixels_length)
    {
        SkImage *internal = get_image(image);
        if (!internal || !pixels || row_bytes > SIZE_MAX)
        {
            return 0;
        }

        const SkImageInfo info = internal->imageInfo().makeColorType(kN32_SkColorType);
        const size_t row_bytes_size = static_cast<size_t>(row_bytes);
        if (!info.validRowBytes(row_bytes_size))
        {
            return 0;
        }

        const size_t required_length = info.computeByteSize(row_bytes_size);
        if (SkImageInfo::ByteSizeOverflowed(required_length) || required_length > pixels_length)
        {
            return 0;
        }

        SkPixmap pixmap(info, pixels, row_bytes_size);
        return internal->readPixels(pixmap, 0, 0) ? 1 : 0;
    }

    AS_API alphaskia_data_t alphaskia_image_encode_png(alphaskia_image_t image)
    {
        SkImage *internal = get_image(image);
        if (!internal)
        {
            return nullptr;
        }

        sk_sp<SkImage> raster = internal->makeRasterImage();
        SkPixmap pixmap;
        if (!raster || !raster->peekPixels(&pixmap))
        {
            return nullptr;
        }

        SkDynamicMemoryWStream stream;
        SkPngEncoder::Options options;
        if (!SkPngEncoder::Encode(static_cast<SkWStream *>(&stream), pixmap, options))
        {
            return nullptr;
        }
        return reinterpret_cast<alphaskia_data_t>(new sk_sp<SkData>(stream.detachAsData()));
    }

    AS_API alphaskia_image_t alphaskia_image_decode(const uint8_t *data, uint64_t length)
    {
        if ((!data && length > 0) || length > SIZE_MAX)
        {
            return nullptr;
        }

        sk_sp<SkData> wrapped_data = SkData::MakeWithCopy(data, static_cast<size_t>(length));
        std::unique_ptr<SkCodec> codec = SkCodec::MakeFromData(wrapped_data);
        if (!codec)
        {
            return nullptr;
        }

        // create desired color type
        SkImageInfo info = codec->getInfo().makeColorType(kN32_SkColorType).makeAlphaType(kPremul_SkAlphaType);
        return wrap_image(std::get<0>(codec->getImage(info)));
    }

    AS_API alphaskia_image_t alphaskia_image_from_pixels(int32_t width, int32_t height, const uint8_t *pixels, uint64_t pixels_length)
    {
        if (!pixels || width <= 0 || height <= 0)
        {
            return nullptr;
        }

        const uint64_t row_bytes = sizeof(uint32_t) * static_cast<uint64_t>(width);
        if (row_bytes * static_cast<uint64_t>(height) > pixels_length)
        {
            return nullptr;
        }

        SkPixmap pixmap(SkImageInfo::Make(width, height, kN32_SkColorType, kPremul_SkAlphaType),
                        pixels, static_cast<size_t>(row_bytes));
        return wrap_image(SkImages::RasterFromPixmapCopy(pixmap));
    }
}
