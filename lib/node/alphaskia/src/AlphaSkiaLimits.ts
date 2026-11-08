import { AlphaSkiaImageFormat, AlphaSkiaLimit, loadAddon } from './addon';

/**
 * Process-wide limits protecting against untrusted input forcing large allocations. Input exceeding a limit is
 * rejected like invalid input (e.g. decoding returns undefined, beginRender throws). A numeric limit of 0 disables it.
 */
export class AlphaSkiaLimits {
    /**
     * The largest width or height in pixels of decoded images, images created from pixels and render surfaces (after
     * the render scale). Default: 32767.
     */
    public static get maxImageDimension(): number {
        return loadAddon().alphaskia_get_limit(AlphaSkiaLimit.MaxImageDimension);
    }

    public static set maxImageDimension(value: number) {
        loadAddon().alphaskia_set_limit(AlphaSkiaLimit.MaxImageDimension, value);
    }

    /**
     * The largest width * height of decoded images, images created from pixels and render surfaces. Default: 2^28.
     */
    public static get maxImagePixels(): number {
        return loadAddon().alphaskia_get_limit(AlphaSkiaLimit.MaxImagePixels);
    }

    public static set maxImagePixels(value: number) {
        loadAddon().alphaskia_set_limit(AlphaSkiaLimit.MaxImagePixels, value);
    }

    /**
     * The largest font data in bytes which can be registered as typeface. Default: 50 MiB.
     */
    public static get maxFontDataLength(): number {
        return loadAddon().alphaskia_get_limit(AlphaSkiaLimit.MaxFontDataLength);
    }

    public static set maxFontDataLength(value: number) {
        loadAddon().alphaskia_set_limit(AlphaSkiaLimit.MaxFontDataLength, value);
    }

    /**
     * The largest font size for drawing and measuring text. Default: 4096.
     */
    public static get maxFontSize(): number {
        return loadAddon().alphaskia_get_limit(AlphaSkiaLimit.MaxFontSize);
    }

    public static set maxFontSize(value: number) {
        loadAddon().alphaskia_set_limit(AlphaSkiaLimit.MaxFontSize, value);
    }

    /**
     * The longest text in UTF-16 code units for drawing and measuring. Default: 0 (unlimited).
     */
    public static get maxTextLength(): number {
        return loadAddon().alphaskia_get_limit(AlphaSkiaLimit.MaxTextLength);
    }

    public static set maxTextLength(value: number) {
        loadAddon().alphaskia_set_limit(AlphaSkiaLimit.MaxTextLength, value);
    }

    /**
     * The image formats {@link AlphaSkiaImage.decode} accepts. Default: {@link AlphaSkiaImageFormat.Png}.
     */
    public static get imageFormats(): AlphaSkiaImageFormat {
        return loadAddon().alphaskia_get_limit(AlphaSkiaLimit.ImageFormats);
    }

    public static set imageFormats(value: AlphaSkiaImageFormat) {
        loadAddon().alphaskia_set_limit(AlphaSkiaLimit.ImageFormats, value);
    }
}
