package alphaTab.alphaSkia.test;

import alphaTab.alphaSkia.AlphaSkiaTextAlign;
import alphaTab.alphaSkia.AlphaSkiaTextBaseline;
import alphaTab.alphaSkia.AlphaSkiaTextStyle;
import alphaTab.alphaSkia.AlphaSkiaTypeface;

import java.io.IOException;
import java.nio.file.Files;
import java.nio.file.Path;
import java.util.Map;
import java.util.TreeMap;

public class AlphaTabGeneratedRenderTestBase {
    private static float renderScale = 1;

    public static float getRenderScale() {
        return renderScale;
    }

    public static void setRenderScale(float renderScale) {
        AlphaTabGeneratedRenderTestBase.renderScale = renderScale;
    }

    private static AlphaSkiaTextStyle musicTextStyle;

    public static AlphaSkiaTextStyle getMusicTextStyle() {
        return musicTextStyle;
    }

    public static void setMusicTextStyle(AlphaSkiaTextStyle musicTextStyle) {
        AlphaTabGeneratedRenderTestBase.musicTextStyle = musicTextStyle;
    }

    public static float getMusicFontSize() {
        return 34;
    }

    private static AlphaSkiaTextAlign textAlign = AlphaSkiaTextAlign.LEFT;

    public static AlphaSkiaTextAlign getTextAlign() {
        return textAlign;
    }

    public static void setTextAlign(AlphaSkiaTextAlign textAlign) {
        AlphaTabGeneratedRenderTestBase.textAlign = textAlign;
    }

    private static AlphaSkiaTextBaseline textBaseline = AlphaSkiaTextBaseline.TOP;

    public static AlphaSkiaTextBaseline getTextBaseline() {
        return textBaseline;
    }

    public static void setTextBaseline(AlphaSkiaTextBaseline textBaseline) {
        AlphaTabGeneratedRenderTestBase.textBaseline = textBaseline;
    }


    private static AlphaSkiaTextStyle textStyle;

    public static AlphaSkiaTextStyle getTextStyle() {
        return textStyle;
    }

    public static void setTextStyle(AlphaSkiaTextStyle textStyle) {
        AlphaTabGeneratedRenderTestBase.textStyle = textStyle;
    }

    private static float fontSize = 12;

    public static float getFontSize() {
        return fontSize;
    }

    public static void setFontSize(float fontSize) {
        AlphaTabGeneratedRenderTestBase.fontSize = fontSize;
    }


    private static final Map<String, AlphaSkiaTextStyle> customTextStyles =
            new TreeMap<String, AlphaSkiaTextStyle>(String.CASE_INSENSITIVE_ORDER);

    private static String customTextStyleKey(String[] fontFamilies, int weight, boolean isItalic) {
        return String.join("_", fontFamilies).toLowerCase() + "_" + weight + "_" + isItalic;
    }

    protected static AlphaSkiaTextStyle getTextStyle(String[] fontFamilies, int weight, boolean isItalic) {
        var key = customTextStyleKey(fontFamilies, weight, isItalic);
        var textStyle = customTextStyles.get(key);
        if (textStyle == null) {
            textStyle = new AlphaSkiaTextStyle(fontFamilies, weight, isItalic);
            customTextStyles.put(key, textStyle);
        }

        return textStyle;
    }

    public static AlphaSkiaTypeface loadTypeface(Path filePath) throws IOException {
        System.out.println("Loading typeface from " + filePath);
        var data = Files.readAllBytes(filePath);

        System.out.println("Read " + data.length + " bytes from file, decoding typeface");
        var typeface = AlphaSkiaTypeface.register(data);

        if (typeface == null) {
            throw new IllegalStateException("Could not create typeface from data");
        }
        return typeface;
    }
}
