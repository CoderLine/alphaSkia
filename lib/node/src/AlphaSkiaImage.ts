import { AlphaSkiaNative } from './AlphaSkiaNative';
import { Addon, AlphaSkiaImageHandle } from './addon';

export class AlphaSkiaImage extends AlphaSkiaNative<AlphaSkiaImageHandle> {
    public get width(): number {
        this.checkDisposed();
        return Addon.alphaskia_image_get_width(this.handle!);
    }

    public get height(): number {
        this.checkDisposed();
        return Addon.alphaskia_image_get_height(this.handle!);
    }

    /**
     * @internal
     */
    constructor(handle: AlphaSkiaImageHandle) {
        super(handle, Addon.alphaskia_image_free);
    }

    public readPixels(): ArrayBuffer | undefined {
        this.checkDisposed();
        return Addon.alphaskia_image_read_pixels(this.handle!);
    }

    public toPng(): ArrayBuffer | undefined {
        this.checkDisposed();
        return Addon.alphaskia_image_encode_png(this.handle!);
    }
}