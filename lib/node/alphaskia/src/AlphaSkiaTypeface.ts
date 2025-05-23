import { AlphaSkiaData } from './AlphaSkiaData';
import { AlphaSkiaNative } from './AlphaSkiaNative';
import { AlphaSkiaTypefaceHandle, loadAddon } from './addon';

/**
 * Represents a typeface to draw text.
 */
export class AlphaSkiaTypeface extends AlphaSkiaNative<AlphaSkiaTypefaceHandle> {
    #data: AlphaSkiaData | undefined;

    #familyName: string | undefined = undefined;

    /**
     * Gets the name of the font family of this typeface.
     */
    public get familyName(): string {
        if (this.#familyName === undefined) {
            this.#familyName = loadAddon().alphaskia_typeface_get_family_name(this.handle!);
        }

        return this.#familyName;
    }

    /**
     * Gets a value indicating whether the typeface is bold.
     */
    public get isBold(): boolean {
        return this.weight >= 600;
    }

    /**
     * Gets the weight of the font.
     */
    public get weight(): number {
        return loadAddon().alphaskia_typeface_get_weight(this.handle!)
    }

    /**
     * Gets a value indicating whether the typeface is italic.
     */
    public get isItalic(): boolean {
        return loadAddon().alphaskia_typeface_is_italic(this.handle!);
    }

    /**
     * @internal
     */
    private constructor(handle: AlphaSkiaTypefaceHandle, data?: AlphaSkiaData) {
        super(handle, loadAddon().alphaskia_typeface_free);
        this.#data = data;
    }

    override [Symbol.dispose]() {
        super[Symbol.dispose]();
        if (this.#data) {
            this.#data[Symbol.dispose]();
            this.#data = undefined;
        }
    }

    /**
     * Register a new custom font from the given binary data containing the data of a font compatible with Skia (e.g. TTF).
     * @param data The raw binary data of the font.
     * @return The loaded typeface to use for text rendering or {@code undefined} if the loading failed.
     */
    public static register(data: ArrayBuffer): AlphaSkiaTypeface | undefined {
        const nativeData = loadAddon().alphaskia_data_new_copy(data);
        if (!nativeData) {
            return undefined;
        }
        const typeface = loadAddon().alphaskia_typeface_register(nativeData);
        if (!typeface) {
            loadAddon().alphaskia_data_free(nativeData);
            return undefined;
        }

        return new AlphaSkiaTypeface(typeface, new AlphaSkiaData(nativeData));
    }

    /**
     * Creates a typeface using the provided information.
     * @param name The name of the typeface.
     * @param bold Whether the bold version of the typeface should be loaded.
     * @param italic Whether the italic version of the typeface should be loaded.
     * @return The typeface if it can be found in the already loaded fonts or the system fonts, otherwise {@code undefined}.
     */
    public static create(name: string, bold: boolean, italic: boolean): AlphaSkiaTypeface | undefined;
    /**
    * Creates a typeface using the provided information.
    * @param name The name of the typeface.
    * @param weight The weight of the typeface
    * @param italic Whether the italic version of the typeface should be loaded.
    * @return The typeface if it can be found in the already loaded fonts or the system fonts, otherwise {@code undefined}.
    */
    public static create(name: string, weight: boolean, italic: boolean): AlphaSkiaTypeface | undefined;
    /**
    * Creates a typeface using the provided information.
    * @param name The name of the typeface.
    * @param weightOrBold The weight of the typeface or a boolean to indicate classical bold fonts.
    * @param italic Whether the italic version of the typeface should be loaded.
    * @return The typeface if it can be found in the already loaded fonts or the system fonts, otherwise {@code undefined}.
    */
    public static create(name: string, weightOrBold: number | boolean, italic: boolean): AlphaSkiaTypeface | undefined {
        let weight: number;
        switch (typeof weightOrBold) {
            case 'number':
                weight = weightOrBold | 0;
                break;
            case 'boolean':
                weight = weightOrBold ? 700 : 400;
                break;
            default:
                throw new TypeError('either a numeric weight or a boolean for bold has to be provided');
        }

        const typeface = loadAddon().alphaskia_typeface_make_from_name(name, weight, italic);
        if (!typeface) {
            return undefined;
        }
        return new AlphaSkiaTypeface(typeface);
    }
}