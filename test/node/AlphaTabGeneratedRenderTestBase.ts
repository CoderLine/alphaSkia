import { AlphaSkiaTextAlign, AlphaSkiaTextBaseline, AlphaSkiaTypeface, AlphaSkiaTextStyle } from "@coderline/alphaskia";
import * as fs from 'fs';

const customTextStyles = new Map<string, AlphaSkiaTextStyle>();
function customTextStyleKey(fontFamilies: string[], weight: number, isItalic: boolean) {
    return `${fontFamilies.join('_').toLowerCase()}_${weight}_${isItalic}`;
}

function getTextStyle(fontFamilies: string[], weight: number, isItalic: boolean): AlphaSkiaTextStyle {
    const key = customTextStyleKey(fontFamilies, weight, isItalic);
    let textStyle = customTextStyles.get(key);
    if (!textStyle) {
        textStyle = new AlphaSkiaTextStyle(fontFamilies, weight, isItalic);
        customTextStyles.set(key, textStyle);
    }
    return textStyle;
}

function loadTypeface(filePath: string): AlphaSkiaTypeface {
    console.log(`Loading typefac from ${filePath}`);
    const data = fs.readFileSync(filePath);

    console.log(`Read ${data.length} bytes from file, decoding typeface`);

    const typeface = AlphaSkiaTypeface.register(data.buffer);
    if (!typeface) {
        throw new Error("Could not create typeface from data")
    }

    return typeface;
}

export default {
    renderScale: 1,
    musicTextStyle: null! as AlphaSkiaTextStyle,
    musicFontSize: 34,
    textAlign: AlphaSkiaTextAlign.Left,
    textBaseline: AlphaSkiaTextBaseline.Top,
    textStyle: null! as AlphaSkiaTextStyle,
    fontSize: 12,
    getTextStyle,
    loadTypeface
};