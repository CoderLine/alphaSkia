import { AlphaSkiaNative } from './AlphaSkiaNative';
import { Addon, AlphaSkiaDataHandle } from './addon';

export class AlphaSkiaData extends AlphaSkiaNative<AlphaSkiaDataHandle> {
    /**
     * @internal
     */
    public constructor(handle: AlphaSkiaDataHandle) {
        super(handle, Addon.alphaskia_data_free);
    }

    public toArray(): ArrayBuffer {
        this.checkDisposed();
        return Addon.alphaskia_data_get_data(this.handle!);
    }
}