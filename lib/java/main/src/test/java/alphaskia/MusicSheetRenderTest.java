package alphaskia;

import org.junit.jupiter.api.BeforeAll;
import org.junit.jupiter.api.Test;

import java.io.*;
import java.nio.file.Path;
import java.util.Arrays;
import java.util.HashMap;
import java.util.Map;

public abstract class MusicSheetRenderTest {
    private static AlphaSkiaTypeface _musicTypeface;
    private static final int _musicFontSize = 34;

    protected static AlphaSkiaTypeface getMusicTypeface() {
        return _musicTypeface;
    }

    protected static int getMusicFontSize() {
        return _musicFontSize;
    }

    private static final Map<String, AlphaSkiaTypeface> _customTypefaces =
            new HashMap<>();

    private static String customTypefaceKey(String fontFamily, boolean isBold, boolean isItalic) {
        return fontFamily.toLowerCase() + "_" + isBold + "_" + isItalic;
    }

    @BeforeAll
    public static void setup() throws IOException {
        var bravura = readFont("font/bravura/Bravura.ttf");

        _musicTypeface = AlphaSkiaTypeface.register(bravura);

        _customTypefaces.put(customTypefaceKey("Roboto", false, false),
                AlphaSkiaTypeface.register(readFont("font/roboto/Roboto-Regular.ttf")));
        _customTypefaces.put(customTypefaceKey("Roboto", true, false),
                AlphaSkiaTypeface.register(readFont("font/roboto/Roboto-Bold.ttf")));
        _customTypefaces.put(customTypefaceKey("Roboto", false, true),
                AlphaSkiaTypeface.register(readFont("font/roboto/Roboto-Italic.ttf")));
        _customTypefaces.put(customTypefaceKey("Roboto", true, true),
                AlphaSkiaTypeface.register(readFont("font/roboto/Roboto-BoldItalic.ttf")));

        _customTypefaces.put(customTypefaceKey("PT Serif", false, false),
                AlphaSkiaTypeface.register(readFont("font/ptserif/PTSerif-Regular.ttf")));
        _customTypefaces.put(customTypefaceKey("PT Serif", true, false),
                AlphaSkiaTypeface.register(readFont("font/ptserif/PTSerif-Bold.ttf")));
        _customTypefaces.put(customTypefaceKey("PT Serif", false, true),
                AlphaSkiaTypeface.register(readFont("font/ptserif/PTSerif-Italic.ttf")));
        _customTypefaces.put(customTypefaceKey("PT Serif", true, true),
                AlphaSkiaTypeface.register(readFont("font/ptserif/PTSerif-BoldItalic.ttf")));
    }

    protected static AlphaSkiaTypeface getTypeface(String name, boolean isBold, boolean isItalic) {
        var face = _customTypefaces.getOrDefault(customTypefaceKey(name, isBold, isItalic), null);
        if (face == null) {
            throw new IllegalStateException("Unknown font requested: " + name);
        }
        return face;
    }


    private AlphaSkiaTextAlign textAlign = AlphaSkiaTextAlign.CENTER;

    protected AlphaSkiaTextAlign getTextAlign() {
        return textAlign;
    }

    protected void setTextAlign(AlphaSkiaTextAlign textAlign) {
        this.textAlign = textAlign;
    }

    private AlphaSkiaTextBaseline textBaseline = AlphaSkiaTextBaseline.TOP;

    protected AlphaSkiaTextBaseline getTextBaseline() {
        return textBaseline;
    }

    protected void setTextBaseline(AlphaSkiaTextBaseline textBaseline) {
        this.textBaseline = textBaseline;
    }

    private AlphaSkiaTypeface typeface;

    protected AlphaSkiaTypeface getTypeface() {
        return typeface;
    }

    protected void setTypeface(AlphaSkiaTypeface typeface) {
        this.typeface = typeface;
    }

    private float fontSize;

    public float getFontSize() {
        return fontSize;
    }

    public void setFontSize(float fontSize) {
        this.fontSize = fontSize;
    }

    private static byte[] readFont(String relativePath) throws IOException {
        var fullPath = Path.of(System.getProperty("testdata.path")).resolve(relativePath);
        try (var stream = new FileInputStream(fullPath.toFile())) {
            return stream.readAllBytes();
        }
    }

    private AlphaSkiaImage renderFullImage() {
        try (var canvas = new AlphaSkiaCanvas()) {
            var images = Arrays.stream(getAllParts()).map(p -> p.render(canvas)).toArray(AlphaSkiaImage[]::new);

            canvas.beginRender(getTotalWidth(), getTotalHeight());

            var partPositions = getPartPositions();
            for (var i = 0; i < images.length; i++) {
                canvas.drawImage(images[i], partPositions[i][0], partPositions[i][1]);
                images[i].close();
            }

            return canvas.endRender();
        }
    }

    @Test
    public void renderTest() throws IOException {
        try (var finalImage = renderFullImage()) {
            var fileName = this.getClass().getSimpleName();
            byte[] png = finalImage.toPng();

            try (var stream = new FileOutputStream(fileName + ".png")) {
                stream.write(png);
            }
        }

    }

    protected abstract int getTotalWidth();

    protected abstract int getTotalHeight();

    protected abstract int[][] getPartPositions();

    protected abstract RenderFunction[] getAllParts();
}

