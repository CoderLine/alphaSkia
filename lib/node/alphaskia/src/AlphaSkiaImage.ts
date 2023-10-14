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
}