/*
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
package net.alphatab.alphaskia.test;


final class PixelMatch {
    // blend semi-transparent color with white
    private static byte blend(byte c, byte a)
    {
        return (byte)(255 + (c - 255) * a);
    }

    private static double rgb2Y(byte r, byte g, byte b)
    {
        return r * 0.29889531 + g * 0.58662247 + b * 0.11448223;
    }

    private static double rgb2I(byte r, byte g, byte b)
    {
        return r * 0.59597799 - g * 0.2741761 - b * 0.32180189;
    }

    private static double rgb2Q(byte r, byte g, byte b)
    {
        return r * 0.21147017 - g * 0.52261711 + b * 0.31114694;
    }

    private static void drawPixel(byte[] output, int pos, PixelMatchColor color)
    {
        //noinspection PointlessArithmeticExpression
        output[pos + 0] = color.b();
        output[pos + 1] = color.g();
        output[pos + 2] = color.r();
        output[pos + 3] = (byte)255;
    }

    private static double colorDelta(byte[] img1, byte[] img2, int k, int m, boolean yOnly)
    {
        //noinspection PointlessArithmeticExpression
        var r1 = img1[k + 0];
        var g1 = img1[k + 1];
        var b1 = img1[k + 2];
        var a1 = img1[k + 3];

        //noinspection PointlessArithmeticExpression
        var r2 = img2[m + 0];
        var g2 = img2[m + 1];
        var b2 = img2[m + 2];
        var a2 = img2[m + 3];

        if (a1 == a2 && r1 == r2 && g1 == g2 && b1 == b2)
            return 0;

        if (a1 < (byte)255)
        {
            a1 /= (byte)255;
            r1 = blend(r1, a1);
            g1 = blend(g1, a1);
            b1 = blend(b1, a1);
        }

        if (a2 < (byte)255)
        {
            a2 /= (byte)255;
            r2 = blend(r2, a2);
            g2 = blend(g2, a2);
            b2 = blend(b2, a2);
        }

        var y = rgb2Y(r1, g1, b1) - rgb2Y(r2, g2, b2);

        if (yOnly)
            return y; // brightness difference only

        var i = rgb2I(r1, g1, b1) - rgb2I(r2, g2, b2);
        var q = rgb2Q(r1, g1, b1) - rgb2Q(r2, g2, b2);

        return 0.5053 * y * y + 0.299 * i * i + 0.1957 * q * q;
    }

    // check if a pixel has 3+ adjacent pixels of the same color.
    private static boolean hasManySiblings(byte[] img, int x1, int y1, int width, int height)
    {
        var distance = 1;
        var x0 = Math.max(x1 - distance, 0);
        var y0 = Math.max(y1 - distance, 0);
        var x2 = Math.min(x1 + distance, width - 1);
        var y2 = Math.min(y1 + distance, height - 1);
        var pos = (y1 * width + x1) * 4;
        var zeroes = x1 == x0 || x1 == x2 || y1 == y0 || y1 == y2 ? 1 : 0;

        // go through 8 adjacent pixels
        for (var x = x0; x <= x2; x++)
        {
            for (var y = y0; y <= y2; y++)
            {
                if (x == x1 && y == y1)
                    continue;

                var pos2 = (y * width + x) * 4;
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

    private static boolean Antialiased(byte[] img, int x1, int y1, int width, int height, byte[] img2)
    {
        var distance = 1;
        var x0 = Math.max(x1 - distance, 0);
        var y0 = Math.max(y1 - distance, 0);
        var x2 = Math.min(x1 + distance, width - 1);
        var y2 = Math.min(y1 + distance, height - 1);
        var pos = (y1 * width + x1) * 4;
        var zeroes = x1 == x0 || x1 == x2 || y1 == y0 || y1 == y2 ? 1 : 0;
        var min = 0.0;
        var max = 0.0;
        var minX = 0;
        var minY = 0;
        var maxX = 0;
        var maxY = 0;

        // go through 8 adjacent pixels
        for (var x = x0; x <= x2; x++)
        {
            for (var y = y0; y <= y2; y++)
            {
                if (x == x1 && y == y1)
                {
                    continue;
                }

                // brightness delta between the center pixel and adjacent one
                var delta = colorDelta(img, img, pos, (y * width + x) * 4, true);

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
                (hasManySiblings(img, minX, minY, width, height) &&
                        hasManySiblings(img2, minX, minY, width, height)) ||
                        (hasManySiblings(img, maxX, maxY, width, height) &&
                                hasManySiblings(img2, maxX, maxY, width, height)));
    }

    public static PixelMatchResult match(byte[] img1, byte[] img2, byte[] diffPixels, int width, int height,
                                         PixelMatchOptions options)
    {
        if (img1.length != img2.length || diffPixels.length != img1.length)
        {
            throw new IllegalStateException("Image sizes do not match");
        }

        if (img1.length != width * height * 4)
        {
            throw new IllegalStateException("Image data size does not match width/height.");
        }

        // check if images are identical
        var len = width * height;
        var identical = true;
        var transparentPixels = 0;

        for (var i = 0; i < len; i++)
        {
            //noinspection PointlessArithmeticExpression
            var img1R = img1[(i * 4) + 0];
            var img1G = img1[(i * 4) + 1];
            var img1B = img1[(i * 4) + 2];
            var img1A = img1[(i * 4) + 3];

            //noinspection PointlessArithmeticExpression
            var img2R = img2[(i * 4) + 0];
            var img2G = img2[(i * 4) + 1];
            var img2B = img2[(i * 4) + 2];
            var img2A = img2[(i * 4) + 3];

            if (img1R != img2R || img1G != img2G || img1B != img2B || img1A != img2A)
            {
                identical = false;
                break;
            }

            if (img1A == 0)
            {
                transparentPixels++;
            }
        }

        if (identical)
        {
            return new PixelMatchResult(len, 0, transparentPixels);
        }

        transparentPixels = 0;

        // maximum acceptable square distance between two colors;
        // 35215 is the maximum possible value for the YIQ difference metric
        var maxDelta = 35215 * options.threshold() * options.threshold();

        var diff = 0;

        // compare each pixel of one image against the other one
        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                var pos = (y * width + x) * 4;

                if (img1[pos + 3] == 0)
                {
                    transparentPixels++;
                }

                // squared YUV distance between colors at this pixel position
                var delta = colorDelta(img1, img2, pos, pos, false);

                // the color difference is above the threshold
                if (delta > maxDelta)
                {
                    // check it's a real rendering difference or just anti-aliasing
                    //noinspection StatementWithEmptyBody
                    if ((Antialiased(img1, x, y, width, height, img2) ||
                            Antialiased(img2, x, y, width, height, img1)))
                    {
                        // match
                    }
                    else
                    {
                        // found substantial difference not caused by anti-aliasing; draw it as red
                        drawPixel(diffPixels, pos, options.diffColor());
                        diff++;
                    }
                }
            }
        }

        // return the number of different pixels
        return new PixelMatchResult(len, diff, transparentPixels);
    }

    private PixelMatch() { }
}


