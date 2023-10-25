import { AlphaSkiaTextAlign, AlphaSkiaTextBaseline, AlphaSkiaTypeface } from "@coderline/alphaskia";
import * as fs from 'fs';

const customTypefaces = new Map<string, AlphaSkiaTypeface>();
function customTypefaceKey(fontFamily: string, isBold: boolean, isItalic: boolean) {
    return `${fontFamily.toLocaleLowerCase()}_${isBold}_${isItalic}`;
}

function getTypeface(fontFamily: string, isBold: boolean, isItalic: boolean): AlphaSkiaTypeface {
    const key = customTypefaceKey(fontFamily, isBold, isItalic);
    const typeface = customTypefaces.get(key);
    if (!typeface) {
        throw new Error(`Unknown font requested: ${key}`);
    }
    return typeface;
}

function loadTypeface(fontFamily: string, isBold: boolean, isItalic: boolean, filePath: string): AlphaSkiaTypeface {
    const key = customTypefaceKey(fontFamily, isBold, isItalic);
    console.log(`Loading typeface ${key} from ${filePath}`);
    const data = fs.readFileSync(filePath);

    console.log(`Read ${data.length} bytes from file, decoding typeface`);

    const typeface = AlphaSkiaTypeface.register(data.buffer);
    if (!typeface) {
        throw new Error("Could not create typeface from data")
    }

    customTypefaces.set(key, typeface);
    return typeface;
}

export default {
    renderScale: 1,
    musicTypeface: null! as AlphaSkiaTypeface,
    musicFontSize: 34,
    textAlign: AlphaSkiaTextAlign.Left,
    textBaseline: AlphaSkiaTextBaseline.Top,
    typeface: null! as AlphaSkiaTypeface,
    fontSize: 12,
    getTypeface,
    loadTypeface
};