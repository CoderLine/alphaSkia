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

export interface PixelMatchColor {
    r: number;
    g: number;
    b: number
}

export interface PixelMatchOptions {
    threshold: number;
    diffColor: PixelMatchColor;
}

export interface PixelMatchResult {
    totalPixels: number;
    differentPixels: number;
    transparentPixels: number;
}

// blend semi-transparent color with white
function blend(c: number, a: number) {
    return 255 + (c - 255) * a;
}
function rgb2y(r: number, g: number, b: number) {
    return r * 0.29889531 + g * 0.58662247 + b * 0.11448223;
}
function rgb2i(r: number, g: number, b: number) {
    return r * 0.59597799 - g * 0.2741761 - b * 0.32180189;
}
function rgb2q(r: number, g: number, b: number) {
    return r * 0.21147017 - g * 0.52261711 + b * 0.31114694;
}

function drawPixel(output: Uint8Array, pos: number, color: PixelMatchColor) {
    output[pos + 0] = color.r;
    output[pos + 1] = color.g;
    output[pos + 2] = color.b;
    output[pos + 3] = 255;
}

function colorDelta(img1: Uint8Array, img2: Uint8Array, k: number, m: number, yOnly: boolean = false) {
    let r1 = img1[k + 0];
    let g1 = img1[k + 1];
    let b1 = img1[k + 2];
    let a1 = img1[k + 3];

    let r2 = img2[m + 0];
    let g2 = img2[m + 1];
    let b2 = img2[m + 2];
    let a2 = img2[m + 3];

    if (a1 === a2 && r1 === r2 && g1 === g2 && b1 === b2) return 0;

    if (a1 < 255) {
        a1 /= 255;
        r1 = blend(r1, a1);
        g1 = blend(g1, a1);
        b1 = blend(b1, a1);
    }

    if (a2 < 255) {
        a2 /= 255;
        r2 = blend(r2, a2);
        g2 = blend(g2, a2);
        b2 = blend(b2, a2);
    }

    const y = rgb2y(r1, g1, b1) - rgb2y(r2, g2, b2);

    if (yOnly) return y; // brightness difference only

    const i = rgb2i(r1, g1, b1) - rgb2i(r2, g2, b2);
    const q = rgb2q(r1, g1, b1) - rgb2q(r2, g2, b2);

    return 0.5053 * y * y + 0.299 * i * i + 0.1957 * q * q;
}

// check if a pixel has 3+ adjacent pixels of the same color.
function hasManySiblings(img: Uint8Array, x1: number, y1: number, width: number, height: number) {
    const distance = 1;
    const x0 = Math.max(x1 - distance, 0);
    const y0 = Math.max(y1 - distance, 0);
    const x2 = Math.min(x1 + distance, width - 1);
    const y2 = Math.min(y1 + distance, height - 1);
    const pos = (y1 * width + x1) * 4;
    let zeroes = x1 === x0 || x1 === x2 || y1 === y0 || y1 === y2 ? 1 : 0;

    // go through 8 adjacent pixels
    for (let x = x0; x <= x2; x++) {
        for (let y = y0; y <= y2; y++) {
            if (x === x1 && y === y1) continue;

            const pos2 = (y * width + x) * 4;
            if (
                img[pos] === img[pos2] &&
                img[pos + 1] === img[pos2 + 1] &&
                img[pos + 2] === img[pos2 + 2] &&
                img[pos + 3] === img[pos2 + 3]
            ) {
                zeroes++;
            }

            if (zeroes > 2) return true;
        }
    }

    return false;
}

// check if a pixel is likely a part of anti-aliasing;
// based on "Anti-aliased Pixel and Intensity Slope Detector" paper by V. Vysniauskas, 2009

function antialiased(img: Uint8Array, x1: number, y1: number, width: number, height: number, img2: Uint8Array) {
    const distance = 1;
    const x0 = Math.max(x1 - distance, 0);
    const y0 = Math.max(y1 - distance, 0);
    const x2 = Math.min(x1 + distance, width - 1);
    const y2 = Math.min(y1 + distance, height - 1);
    const pos = (y1 * width + x1) * 4;
    let zeroes = x1 === x0 || x1 === x2 || y1 === y0 || y1 === y2 ? 1 : 0;
    let min = 0;
    let max = 0;
    let minX = 0;
    let minY = 0;
    let maxX = 0;
    let maxY = 0;

    // go through 8 adjacent pixels
    for (let x = x0; x <= x2; x++) {
        for (let y = y0; y <= y2; y++) {
            if (x === x1 && y === y1) continue;

            // brightness delta between the center pixel and adjacent one
            const delta = colorDelta(img, img, pos, (y * width + x) * 4, true);

            // count the number of equal, darker and brighter adjacent pixels
            if (delta === 0) {
                zeroes++;
                // if found more than 2 equal siblings, it's definitely not anti-aliasing
                if (zeroes > 2) return false;

                // remember the darkest pixel
            } else if (delta < min) {
                min = delta;
                minX = x;
                minY = y;

                // remember the brightest pixel
            } else if (delta > max) {
                max = delta;
                maxX = x;
                maxY = y;
            }
        }
    }

    // if there are no both darker and brighter pixels among siblings, it's not anti-aliasing
    if (min === 0 || max === 0) return false;

    // if either the darkest or the brightest pixel has 3+ equal siblings in both images
    // (definitely not anti-aliased), this pixel is anti-aliased
    return (
        (hasManySiblings(img, minX, minY, width, height) &&
            hasManySiblings(img2, minX, minY, width, height)) ||
        (hasManySiblings(img, maxX, maxY, width, height) &&
            hasManySiblings(img2, maxX, maxY, width, height))
    );
}


export function match(
    img1: Uint8Array,
    img2: Uint8Array,
    output: Uint8Array,
    width: number,
    height: number,
    options: PixelMatchOptions
): PixelMatchResult {
    if (img1.length !== img2.length || (output && output.length !== img1.length)) {
        throw new Error(`Image sizes do not match`);
    }

    if (img1.length !== width * height * 4) throw new Error('Image data size does not match width/height.');

    // check if images are identical
    const len = width * height;
    let identical = true;
    let transparentPixels = 0;

    for (let i = 0; i < len; i++) {
        const img1r = img1[(i * 4) + 0];
        const img1g = img1[(i * 4) + 1];
        const img1b = img1[(i * 4) + 2];
        const img1a = img1[(i * 4) + 3];

        const img2r = img2[(i * 4) + 0];
        const img2g = img2[(i * 4) + 1];
        const img2b = img2[(i * 4) + 2];
        const img2a = img2[(i * 4) + 3];

        if (img1r !== img2r || img1g !== img2g || img1b !== img2b || img1a !== img2a) {
            identical = false;
            break;
        }
        if (img1a === 0) {
            transparentPixels++;
        }
    }
    if (identical) {
        return {
            totalPixels: len,
            differentPixels: 0,
            transparentPixels: transparentPixels
        };
    }

    transparentPixels = 0;

    // maximum acceptable square distance between two colors;
    // 35215 is the maximum possible value for the YIQ difference metric
    const maxDelta = 35215 * options.threshold! * options.threshold!;

    let diff = 0;

    // compare each pixel of one image against the other one
    for (let y = 0; y < height; y++) {
        for (let x = 0; x < width; x++) {
            const pos = (y * width + x) * 4;

            if (img1[pos + 3] === 0) {
                transparentPixels++;
            }

            // squared YUV distance between colors at this pixel position
            const delta = colorDelta(img1, img2, pos, pos);

            // the color difference is above the threshold
            if (delta > maxDelta) {
                // check it's a real rendering difference or just anti-aliasing
                if ((antialiased(img1, x, y, width, height, img2) ||
                    antialiased(img2, x, y, width, height, img1))
                ) {
                    // match
                } else {
                    // found substantial difference not caused by anti-aliasing; draw it as red
                    drawPixel(output, pos, options.diffColor);
                    diff++;
                }
            }
        }
    }

    // return the number of different pixels
    return {
        totalPixels: len,
        differentPixels: diff,
        transparentPixels: transparentPixels
    };
}