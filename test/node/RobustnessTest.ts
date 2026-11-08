import { AlphaSkiaCanvas, AlphaSkiaImage, AlphaSkiaImageFormat, AlphaSkiaLimits, AlphaSkiaTextAlign, AlphaSkiaTextBaseline, AlphaSkiaTextStyle, AlphaSkiaTypeface } from '@coderline/alphaskia';

// Invalid input must be rejected with undefined or a JavaScript error instead of crashing the process.
export function robustnessTest(): boolean {
    console.log("BEGIN: robustness test");
    let checks = 0;
    let failures = 0;
    function check(condition: boolean, message: string) {
        checks++;
        if (!condition) {
            failures++;
            console.error(`  FAILED: ${message}`);
        }
    }
    function throws(action: () => void, errorType: Function, message: string) {
        try {
            action();
            check(false, `${message}: no error`);
        } catch (e) {
            check(e instanceof errorType, `${message}: ${e}`);
        }
    }

    const garbage = new Uint8Array([0, 1, 2, 3, 4, 5, 6, 7, 0xff, 0xfe]).buffer;
    check(AlphaSkiaTypeface.register(garbage) === undefined, "typeface from garbage bytes");
    check(AlphaSkiaImage.decode(garbage) === undefined, "decode garbage bytes");
    check(AlphaSkiaImage.fromPixels(1000, 1000, new ArrayBuffer(16)) === undefined, "pixels larger than the buffer");
    check(AlphaSkiaImage.fromPixels(-1, 1, new ArrayBuffer(16)) === undefined, "pixels with negative width");

    throws(() => new AlphaSkiaTextStyle(new Array(256).fill("Noto Sans"), 400, false), RangeError, "256 font families");

    using style = new AlphaSkiaTextStyle(["Noto Sans"], 400, false);
    using canvas = new AlphaSkiaCanvas();
    throws(() => canvas.beginRender(10, 10, 0), RangeError, "render scale 0");
    throws(() => canvas.beginRender(10, 10, NaN), RangeError, "render scale NaN");
    throws(() => canvas.beginRender(NaN, 10), RangeError, "width NaN");
    throws(() => canvas.beginRender(2 ** 32 + 5, 10), RangeError, "width beyond int32");
    canvas.beginRender(10, 10);

    throws(() => canvas.measureText("a", style, 12, 7 as AlphaSkiaTextAlign, AlphaSkiaTextBaseline.Alphabetic), RangeError, "text align out of range");

    const metrics = canvas.measureText("a", style, 12, AlphaSkiaTextAlign.Left, AlphaSkiaTextBaseline.Alphabetic);
    metrics[Symbol.dispose]();
    throws(() => metrics.width, ReferenceError, "width of disposed metrics");

    const image = AlphaSkiaImage.fromPixels(1, 1, new ArrayBuffer(4))!;
    image[Symbol.dispose]();
    image[Symbol.dispose]();
    throws(() => canvas.drawImage(image, 0, 0, 1, 1), ReferenceError, "draw disposed image");

    const disposedStyle = new AlphaSkiaTextStyle(["Noto Sans"], 400, false);
    disposedStyle[Symbol.dispose]();
    throws(() => canvas.fillText("a", disposedStyle, 12, 0, 0, AlphaSkiaTextAlign.Left, AlphaSkiaTextBaseline.Alphabetic), ReferenceError, "fill text with disposed style");

    check(AlphaSkiaLimits.maxImageDimension === 32767, "default max image dimension");
    check(AlphaSkiaLimits.imageFormats === AlphaSkiaImageFormat.Png, "default image formats");
    throws(() => { AlphaSkiaLimits.maxFontSize = -1; }, RangeError, "negative limit");
    AlphaSkiaLimits.maxImageDimension = 100;
    throws(() => canvas.beginRender(200, 10), RangeError, "render above dimension limit");
    AlphaSkiaLimits.maxImageDimension = 32767;
    canvas.beginRender(10, 10);

    canvas.endRender()?.[Symbol.dispose]();

    console.log(`  ${checks - failures}/${checks} checks passed`);
    if (failures > 0) {
        console.log("FAILED: robustness test");
        return false;
    }
    console.log("SUCCESS: robustness test");
    return true;
}
