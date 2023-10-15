/**
 * Based on https://github.com/mapbox/pixelmatch
 * ISC License
 * Copyright (c) 2019, Mapbox
 *
 * Permission to use, copy, modify, and/or distribute this software for any purpose
 * with or without fee is hereby granted, provided that the above copyright notice
 * and this permission notice appear in all copies.
 *
 * THE SOFTWARE IS PROVIDED "AS IS" AND THE AUTHOR DISCLAIMS ALL WARRANTIES WITH
 * REGARD TO THIS SOFTWARE INCLUDING ALL IMPLIED WARRANTIES OF MERCHANTABILITY AND
 * FITNESS. IN NO EVENT SHALL THE AUTHOR BE LIABLE FOR ANY SPECIAL, DIRECT,
 * INDIRECT, OR CONSEQUENTIAL DAMAGES OR ANY DAMAGES WHATSOEVER RESULTING FROM LOSS
 * OF USE, DATA OR PROFITS, WHETHER IN AN ACTION OF CONTRACT, NEGLIGENCE OR OTHER
 * TORTIOUS ACTION, ARISING OUT OF OR IN CONNECTION WITH THE USE OR PERFORMANCE OF
 * THIS SOFTWARE.
 */
#include "../include/PixelMatch.h"
#include <vector>
#include <iostream>

// blend semi-transparent color with white
uint8_t blend(uint8_t c, uint8_t a)
{
    return 255 + (c - 255) * a;
}

double rgb2y(uint8_t r, uint8_t g, uint8_t b)
{
    return r * 0.29889531 + g * 0.58662247 + b * 0.11448223;
}
double rgb2i(uint8_t r, uint8_t g, uint8_t b)
{
    return r * 0.59597799 - g * 0.2741761 - b * 0.32180189;
}
double rgb2q(uint8_t r, uint8_t g, uint8_t b)
{
    return r * 0.21147017 - g * 0.52261711 + b * 0.31114694;
}

void draw_pixel(std::vector<uint8_t> &output, int32_t pos, pixel_match_color_t color)
{
    output[pos + 0] = color.b;
    output[pos + 1] = color.g;
    output[pos + 2] = color.r;
    output[pos + 3] = 255;
}

double color_delta(const std::vector<uint8_t> &img1,
                   const std::vector<uint8_t> &img2,
                   int32_t k,
                   int32_t m,
                   bool yOnly)
{
    uint8_t r1 = img1[k + 0];
    uint8_t g1 = img1[k + 1];
    uint8_t b1 = img1[k + 2];
    uint8_t a1 = img1[k + 3];

    uint8_t r2 = img2[m + 0];
    uint8_t g2 = img2[m + 1];
    uint8_t b2 = img2[m + 2];
    uint8_t a2 = img2[m + 3];

    if (a1 == a2 && r1 == r2 && g1 == g2 && b1 == b2)
        return 0;

    if (a1 < 255)
    {
        a1 /= 255;
        r1 = blend(r1, a1);
        g1 = blend(g1, a1);
        b1 = blend(b1, a1);
    }

    if (a2 < 255)
    {
        a2 /= 255;
        r2 = blend(r2, a2);
        g2 = blend(g2, a2);
        b2 = blend(b2, a2);
    }

    const double y = rgb2y(r1, g1, b1) - rgb2y(r2, g2, b2);

    if (yOnly)
        return y; // brightness difference only

    const double i = rgb2i(r1, g1, b1) - rgb2i(r2, g2, b2);
    const double q = rgb2q(r1, g1, b1) - rgb2q(r2, g2, b2);

    return 0.5053 * y * y + 0.299 * i * i + 0.1957 * q * q;
}

// check if a pixel has 3+ adjacent pixels of the same color.
bool has_many_siblings(const std::vector<uint8_t> &img, int32_t x1, int32_t y1, int32_t width, int32_t height)
{
    const int32_t distance = 1;
    const int32_t x0 = std::max<int32_t>(x1 - distance, 0);
    const int32_t y0 = std::max<int32_t>(y1 - distance, 0);
    const int32_t x2 = std::min<int32_t>(x1 + distance, width - 1);
    const int32_t y2 = std::min<int32_t>(y1 + distance, height - 1);
    const int32_t pos = (y1 * width + x1) * 4;
    int32_t zeroes = x1 == x0 || x1 == x2 || y1 == y0 || y1 == y2 ? 1 : 0;

    // go through 8 adjacent pixels
    for (int32_t x = x0; x <= x2; x++)
    {
        for (int32_t y = y0; y <= y2; y++)
        {
            if (x == x1 && y == y1)
                continue;

            const int32_t pos2 = (y * width + x) * 4;
            if (
                img[pos] == img[pos2] &&
                img[pos + 1] == img[pos2 + 1] &&
                img[pos + 2] == img[pos2 + 2] &&
                img[pos + 3] == img[pos2 + 3])
            {
                zeroes++;
            }

            if (zeroes > 2)
                return true;
        }
    }

    return false;
}

// check if a pixel is likely a part of anti-aliasing;
// based on "Anti-aliased Pixel and Intensity Slope Detector" paper by V. Vysniauskas, 2009

bool antialiased(const std::vector<uint8_t> &img, int32_t x1, int32_t y1, int32_t width, int32_t height, const std::vector<uint8_t> &img2)
{
    const int32_t distance = 1;
    const int32_t x0 = std::max<int32_t>(x1 - distance, 0);
    const int32_t y0 = std::max<int32_t>(y1 - distance, 0);
    const int32_t x2 = std::min<int32_t>(x1 + distance, width - 1);
    const int32_t y2 = std::min<int32_t>(y1 + distance, height - 1);
    const int32_t pos = (y1 * width + x1) * 4;
    int32_t zeroes = x1 == x0 || x1 == x2 || y1 == y0 || y1 == y2 ? 1 : 0;
    double min = 0;
    double max = 0;
    int32_t minX = 0;
    int32_t minY = 0;
    int32_t maxX = 0;
    int32_t maxY = 0;

    // go through 8 adjacent pixels
    for (int32_t x = x0; x <= x2; x++)
    {
        for (int32_t y = y0; y <= y2; y++)
        {
            if (x == x1 && y == y1)
            {
                continue;
            }

            // brightness delta between the center pixel and adjacent one
            const double delta = color_delta(img, img, pos, (y * width + x) * 4, true);

            // count the number of equal, darker and brighter adjacent pixels
            if (delta == 0)
            {
                zeroes++;
                // if found more than 2 equal siblings, it's definitely not anti-aliasing
                if (zeroes > 2)
                    return false;

                // remember the darkest pixel
            }
            else if (delta < min)
            {
                min = delta;
                minX = x;
                minY = y;

                // remember the brightest pixel
            }
            else if (delta > max)
            {
                max = delta;
                maxX = x;
                maxY = y;
            }
        }
    }

    // if there are no both darker and brighter pixels among siblings, it's not anti-aliasing
    if (min == 0 || max == 0)
    {
        return false;
    }

    // if either the darkest or the brightest pixel has 3+ equal siblings in both images
    // (definitely not anti-aliased), this pixel is anti-aliased
    return (
        (has_many_siblings(img, minX, minY, width, height) &&
         has_many_siblings(img2, minX, minY, width, height)) ||
        (has_many_siblings(img, maxX, maxY, width, height) &&
         has_many_siblings(img2, maxX, maxY, width, height)));
}

pixel_match_result_t pixel_match(const std::vector<uint8_t> &img1,
                                 const std::vector<uint8_t> &img2,
                                 std::vector<uint8_t> &diff_pixels,
                                 int32_t width,
                                 int32_t height,
                                 pixel_match_options_t options)
{
    if (img1.size() != img2.size() || diff_pixels.size() != img1.size())
    {
        throw std::exception("Image sizes do not match");
    }

    if (img1.size() != width * height * sizeof(uint32_t))
    {
        throw std::exception("Image data size does not match width/height.");
    }

    // check if images are identical
    const uint32_t len = width * height;
    bool identical = true;
    uint32_t transparentPixels = 0;

    for (uint32_t i = 0; i < len; i++)
    {
        const uint8_t img1r = img1[(i * 4) + 0];
        const uint8_t img1g = img1[(i * 4) + 1];
        const uint8_t img1b = img1[(i * 4) + 2];
        const uint8_t img1a = img1[(i * 4) + 3];

        const uint8_t img2r = img2[(i * 4) + 0];
        const uint8_t img2g = img2[(i * 4) + 1];
        const uint8_t img2b = img2[(i * 4) + 2];
        const uint8_t img2a = img2[(i * 4) + 3];

        if (img1r != img2r || img1g != img2g || img1b != img2b || img1a != img2a)
        {
            identical = false;
            break;
        }
        if (img1a == 0)
        {
            transparentPixels++;
        }
    }

    if (identical)
    {
        return {len, 0, transparentPixels};
    }

    transparentPixels = 0;

    // maximum acceptable square distance between two colors;
    // 35215 is the maximum possible value for the YIQ difference metric
    const double maxDelta = 35215 * options.threshold * options.threshold;

    uint32_t diff = 0;

    // compare each pixel of one image against the other one
    for (int32_t y = 0; y < height; y++)
    {
        for (int32_t x = 0; x < width; x++)
        {
            const int32_t pos = (y * width + x) * sizeof(uint32_t);

            if (img1[pos + 3] == 0)
            {
                transparentPixels++;
            }
            // squared YUV distance between colors at this pixel position
            const double delta = color_delta(img1, img2, pos, pos, false);

            // the color difference is above the threshold
            if (delta > maxDelta)
            {
                // check it's a real rendering difference or just anti-aliasing
                if ((antialiased(img1, x, y, width, height, img2) ||
                     antialiased(img2, x, y, width, height, img1)))
                {
                    // match
                }
                else
                {
                    // found substantial difference not caused by anti-aliasing; draw it as red
                    draw_pixel(diff_pixels, pos, options.diff_color);
                    diff++;
                }
            }
        }
    }

    // return the number of different pixels
    return {len, diff, transparentPixels};
}