package alphaTab.alphaSkia.test;

import alphaTab.alphaSkia.AlphaSkiaTextAlign;
import alphaTab.alphaSkia.AlphaSkiaTextBaseline;
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

    private static AlphaSkiaTypeface musicTypeface;

    public static AlphaSkiaTypeface getMusicTypeface() {
        return musicTypeface;
    }

    public static void setMusicTypeface(AlphaSkiaTypeface musicTypeface) {
        AlphaTabGeneratedRenderTestBase.musicTypeface = musicTypeface;
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


    private static AlphaSkiaTypeface typeface;

    public static AlphaSkiaTypeface getTypeface() {
        return typeface;
    }

    public static void setTypeface(AlphaSkiaTypeface typeface) {
        AlphaTabGeneratedRenderTestBase.typeface = typeface;
    }

    private static float fontSize = 12;

    public static float getFontSize() {
        return fontSize;
    }

    public static void setFontSize(float fontSize) {
        AlphaTabGeneratedRenderTestBase.fontSize = fontSize;
    }


    private static final Map<String, AlphaSkiaTypeface> customTypefaces =
            new TreeMap<String, AlphaSkiaTypeface>(String.CASE_INSENSITIVE_ORDER);

    private static String customTypefaceKey(String fontFamily, boolean isBold, boolean isItalic) {
        return fontFamily.toLowerCase() + "_" + isBold + "_" + isItalic;
    }

    protected static AlphaSkiaTypeface getTypeface(String fontFamily, boolean isBold, boolean isItalic) {
        var key = customTypefaceKey(fontFamily, isBold, isItalic);
        var typeface = customTypefaces.get(key);
        if (typeface == null) {
            throw new IllegalStateException("Unknown font requested: " + key);
        }

        return typeface;
    }

    public static AlphaSkiaTypeface loadTypeface(String name, boolean isBold, boolean isItalic, Path filePath) throws IOException {
        var key = customTypefaceKey(name, isBold, isItalic);
        System.out.println("Loading typeface " + key + " from " + filePath);
        var data = Files.readAllBytes(filePath);

        System.out.println("Read " + data.length + " bytes from file, decoding typeface");
        var typeface = AlphaSkiaTypeface.register(data);

        if (typeface == null) {
            throw new IllegalStateException("Could not create typeface from data");
        }
        customTypefaces.put(key, typeface);
        return typeface;
    }
}
