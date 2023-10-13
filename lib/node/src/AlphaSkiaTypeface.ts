import { AlphaSkiaData } from './AlphaSkiaData';
import { AlphaSkiaNative } from './AlphaSkiaNative';
import { Addon, AlphaSkiaTypefaceHandle } from './addon';

export class AlphaSkiaTypeface extends AlphaSkiaNative<AlphaSkiaTypefaceHandle> {
    #data: AlphaSkiaData | undefined;

    /**
     * @internal
     */
    public constructor(handle: AlphaSkiaTypefaceHandle, data?: AlphaSkiaData) {
        super(handle, Addon.alphaskia_typeface_free);
        this.#data = data;
    }

    [Symbol.dispose]() {
        super[Symbol.dispose]();
        if (this.#data) {
            this.#data[Symbol.dispose]();
            this.#data = undefined;
        }
    }

    public toArray(): ArrayBuffer {
        this.checkDisposed();
        return Addon.alphaskia_data_get_data(this.handle!);
    }

    public static register(data: ArrayBuffer): AlphaSkiaTypeface | undefined {
        const nativeData = Addon.alphaskia_data_new_copy(data);
        if (!nativeData) {
            return undefined;
        }
        const typeface = Addon.alphaskia_typeface_register(nativeData);
        if (!typeface) {
            Addon.alphaskia_data_free(nativeData);
            return undefined;
        }

        return new AlphaSkiaTypeface(typeface, new AlphaSkiaData(nativeData));
    }

    public static create(name: string, bold: boolean, italic: boolean): AlphaSkiaTypeface | undefined {
        const typeface = Addon.alphaskia_typeface_make_from_name(name, bold, italic);
        if (!typeface) {
            return undefined;
        }
        return new AlphaSkiaTypeface(typeface);
    }
}