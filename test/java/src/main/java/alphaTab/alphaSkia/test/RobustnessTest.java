package alphaTab.alphaSkia.test;

import alphaTab.alphaSkia.*;

/**
 * Invalid input must be rejected with null or a Java exception instead of crashing the JVM.
 */
public class RobustnessTest {
    private int checks = 0;
    private int failures = 0;

    private void check(boolean condition, String message) {
        checks++;
        if (!condition) {
            failures++;
            System.err.println("  FAILED: " + message);
        }
    }

    private void expectThrows(Runnable action, Class<? extends Throwable> errorType, String message) {
        try {
            action.run();
            check(false, message + ": no exception");
        } catch (Throwable e) {
            check(errorType.isInstance(e), message + ": " + e);
        }
    }

    public static boolean run() {
        return new RobustnessTest().runChecks();
    }

    private boolean runChecks() {
        System.out.println("BEGIN: robustness test");
        var garbage = new byte[]{0, 1, 2, 3, 4, 5, 6, 7, (byte) 0xff, (byte) 0xfe};

        check(AlphaSkiaTypeface.register(garbage) == null, "typeface from garbage bytes");
        check(AlphaSkiaImage.decode(garbage) == null, "decode garbage bytes");
        check(AlphaSkiaImage.fromPixels(1000, 1000, new byte[16]) == null, "pixels larger than the buffer");
        check(AlphaSkiaImage.fromPixels(-1, 1, new byte[16]) == null, "pixels with negative width");
        expectThrows(() -> AlphaSkiaImage.decode(null), NullPointerException.class, "decode null");
        expectThrows(() -> AlphaSkiaTypeface.create(null, 400, false), NullPointerException.class, "typeface without name");
        expectThrows(() -> new AlphaSkiaTextStyle(new String[256], 400, false), IllegalArgumentException.class, "256 font families");
        expectThrows(() -> new AlphaSkiaTextStyle(new String[]{null}, 400, false), NullPointerException.class, "null font family");
        expectThrows(() -> new AlphaSkiaTextStyle(new String[]{"Noto Sans"}, 5000, false), IllegalArgumentException.class, "weight out of range");

        try (var style = new AlphaSkiaTextStyle(new String[]{"Noto Sans"}, 400, false);
             var canvas = new AlphaSkiaCanvas()) {
            expectThrows(() -> canvas.beginRender(10, 10, 0f), IllegalArgumentException.class, "render scale 0");
            expectThrows(() -> canvas.beginRender(10, 10, Float.NaN), IllegalArgumentException.class, "render scale NaN");
            canvas.beginRender(10, 10);

            expectThrows(() -> canvas.fillText(null, style, 12, 0, 0, AlphaSkiaTextAlign.LEFT, AlphaSkiaTextBaseline.ALPHABETIC), NullPointerException.class, "fill null text");
            expectThrows(() -> canvas.fillText("a", null, 12, 0, 0, AlphaSkiaTextAlign.LEFT, AlphaSkiaTextBaseline.ALPHABETIC), NullPointerException.class, "fill text without style");
            expectThrows(() -> canvas.measureText("a", style, Float.POSITIVE_INFINITY, AlphaSkiaTextAlign.LEFT, AlphaSkiaTextBaseline.ALPHABETIC), IllegalArgumentException.class, "measure infinite font size");
            expectThrows(() -> canvas.drawImage(null, 0, 0, 1, 1), NullPointerException.class, "draw null image");

            var image = AlphaSkiaImage.fromPixels(1, 1, new byte[4]);
            check(image != null, "pixels 1x1");
            image.close();
            image.close();
            expectThrows(() -> canvas.drawImage(image, 0, 0, 1, 1), IllegalArgumentException.class, "draw closed image");

            var closedStyle = new AlphaSkiaTextStyle(new String[]{"Noto Sans"}, 400, false);
            closedStyle.close();
            expectThrows(() -> canvas.fillText("a", closedStyle, 12, 0, 0, AlphaSkiaTextAlign.LEFT, AlphaSkiaTextBaseline.ALPHABETIC), IllegalArgumentException.class, "fill text with closed style");

            check(AlphaSkiaLimits.getMaxImageDimension() == 32767, "default max image dimension");
            check(AlphaSkiaLimits.getImageFormats().equals(java.util.EnumSet.of(AlphaSkiaImageFormat.PNG)), "default image formats");
            expectThrows(() -> AlphaSkiaLimits.setMaxFontSize(-1), IllegalArgumentException.class, "negative limit");
            AlphaSkiaLimits.setMaxImageDimension(100);
            expectThrows(() -> canvas.beginRender(200, 10), IllegalArgumentException.class, "render above dimension limit");
            AlphaSkiaLimits.setMaxImageDimension(32767);
            canvas.beginRender(10, 10);

            try (var rendered = canvas.endRender()) {
                check(rendered != null, "end render");
            }
        }

        System.out.println("  " + (checks - failures) + "/" + checks + " checks passed");
        if (failures > 0) {
            System.out.println("FAILED: robustness test");
            return false;
        }
        System.out.println("SUCCESS: robustness test");
        return true;
    }
}
