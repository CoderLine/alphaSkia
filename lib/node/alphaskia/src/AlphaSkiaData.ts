import { AlphaSkiaNative } from './AlphaSkiaNative';
import { AlphaSkiaDataHandle, loadAddon } from './addon';

export class AlphaSkiaData extends AlphaSkiaNative<AlphaSkiaDataHandle> {
    /**
     * @internal
     */
    public constructor(handle: AlphaSkiaDataHandle) {
        super(handle, loadAddon().alphaskia_data_free);
    }

    public toArray(): ArrayBuffer {
        this.checkDisposed();
        return loadAddon().alphaskia_data_get_data(this.handle!);
    }
}