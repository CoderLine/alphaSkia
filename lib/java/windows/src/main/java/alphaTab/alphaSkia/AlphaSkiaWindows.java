package alphaTab.alphaSkia;

/**
 * The alphaSkia initializer for Windows.
 */
public class AlphaSkiaWindows extends AlphaSkiaJre {
    /**
     * The alphaSkia initializer for Windows.
     */
    public static final AlphaSkiaWindows INSTANCE = new AlphaSkiaWindows();

    private AlphaSkiaWindows() {
    }

    @Override
    protected String[] getJavaResources() {
        return new String[]{
                "/native/win-" + getCurrentArchitecture() + "/libalphaskiajni.dll"
        };
    }
}