import { AlphaSkiaNative } from './AlphaSkiaNative';
import { AlphaSkiaTypefaceHandle, loadAddon } from './addon';

/**
 * Represents a text style which can be used to draw or measure texts 
 * with support for mixed fonts for fallback character rendering.
 */
export class AlphaSkiaTextStyle extends AlphaSkiaNative<AlphaSkiaTypefaceHandle> {
    /**
     * Gets the list of font family names which are consulted for finding 
     * typefaces with glyphs for drawing or measuring texts.
     */
    public readonly familyNames: string[];

    /**
     * Gets the font weight used for finding typefaces.
     */
    public readonly weight: number;

    /**
     * Gets whether the used typefaces should be italic. 
     */
    public readonly isItalic: boolean;

    /**
     * Initializes a new instance of the {@link AlphaSkiaTextStyle} class.
     * @param familyNames The list of font family names.
     * @param weight The font weight typefaces should have.
     * @param isItalic Whether typefaces should be italic.
     */
    public constructor(familyNames: string[], weight: number, isItalic: boolean) {
        super(
            loadAddon().alphaskia_textstyle_new(AlphaSkiaTextStyle.checkFamilyNames(familyNames), weight, isItalic),
            loadAddon().alphaskia_textstyle_free,
        )
        this.familyNames = familyNames;
        this.weight = weight;
        this.isItalic = isItalic;
    }


    private static checkFamilyNames(familyNames: unknown): string[] {
        if (!Array.isArray(familyNames)) {
            throw new TypeError("family names must be a string array, provided value is no array");
        }

        for (let i = 0; i < familyNames.length; i++) {
            if (typeof familyNames[i] !== 'string') {
                throw new TypeError(`family names must be a string array, provided value at index ${i} is not a string`);
            }
        }

        return familyNames as string[];
    }
}