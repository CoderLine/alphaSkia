package alphaTab.alphaSkia;

/**
 * The alphaSkia initializer for macOS.
 */
public class AlphaSkiaMacOs extends AlphaSkiaJre {
    /**
     * The alphaSkia initializer for macOS.
     */
    public static final AlphaSkiaMacOs INSTANCE = new AlphaSkiaMacOs();

    private AlphaSkiaMacOs() {
    }

    @Override
    protected String[] getJavaResources() {
        return new String[] {
            "/native/macos-" + getCurrentArchitecture() + "/libalphaskiajni.dylib"
        };
    }
}