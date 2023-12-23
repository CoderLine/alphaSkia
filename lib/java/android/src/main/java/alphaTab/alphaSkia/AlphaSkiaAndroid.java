package alphaTab.alphaSkia;

/**
 * The alphaSkia initializer for Android.
 */
public class AlphaSkiaAndroid extends AlphaSkiaPlatform {
    public static final AlphaSkiaAndroid INSTANCE = new AlphaSkiaAndroid();

    @Override
    public void inititalize() {
        System.loadLibrary("alphaskiajni");
        setNativeLibLoaded();
    }
}

