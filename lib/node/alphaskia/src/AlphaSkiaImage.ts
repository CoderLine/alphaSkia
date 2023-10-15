import { AlphaSkiaNative } from './AlphaSkiaNative';
import { AlphaSkiaImageHandle, loadAddon } from './addon';

/**
 * Represents a final rendered image.
 */
export class AlphaSkiaImage extends AlphaSkiaNative<AlphaSkiaImageHandle> {
    /**
     * Gets the width of the image.
     *
     * @return the width of the image
     */
    public get width(): number {
        this.checkDisposed();
        return loadAddon().alphaskia_image_get_width(this.handle!);
    }

    /**
     * Gets the height of the image.
     *
     * @return the height of the image
     */
    public get height(): number {
        this.checkDisposed();
        return loadAddon().alphaskia_image_get_height(this.handle!);
    }

    /**
     * @internal
     */
    constructor(handle: AlphaSkiaImageHandle) {
        super(handle, loadAddon().alphaskia_image_free);
    }

    /**
     * Reads the raw pixel data of this image as byte array.
     *
     * @return A copy of the raw pixel data.
     */
    public readPixels(): ArrayBuffer | undefined {
        this.checkDisposed();
        return loadAddon().alphaskia_image_read_pixels(this.handle!);
    }

    /**
     * Encodes the image to a PNG.
     *
     * @return The raw PNG bytes for further usage.
     */
    public toPng(): ArrayBuffer | undefined {
        this.checkDisposed();
        return loadAddon().alphaskia_image_encode_png(this.handle!);
    }

    /**
     * Decodes the given bytes into an image using supported image formats like PNG.
     * @param bytes The raw iamge bytes.
     * @return The decoded image or {@code undefined} if the image could not be decoded.
     */
    public static decode(bytes: ArrayBuffer): AlphaSkiaImage | undefined {
        const handle = loadAddon().alphaskia_image_decode(bytes);
        return !handle ? undefined : new AlphaSkiaImage(handle);
    }


    /**
     * Creates an image from the raw pixels assuming the default internal pixel format of alphaSkia.
     * @param width The width of the image in pixels.
     * @param height The height of the image in pixels.
     * @param pixels The raw pixel bytes.
     * @return The decoded image or {@code undefined} if the creation of the image from the pixels failed.
     */
    public static fromPixels(width: number, height: number, pixels: ArrayBuffer): AlphaSkiaImage | undefined {
        const handle = loadAddon().alphaskia_image_from_pixels(width, height, pixels);
        return handle == 0 ? undefined : new AlphaSkiaImage(handle);
    }
}