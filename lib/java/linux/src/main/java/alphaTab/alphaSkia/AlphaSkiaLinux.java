package alphaTab.alphaSkia;

/**
 * The alphaSkia initializer for Linux.
 */
public class AlphaSkiaLinux extends AlphaSkiaJre {
    /**
     * The alphaSkia initializer for Linux.
     */
    public static final AlphaSkiaLinux INSTANCE = new AlphaSkiaLinux();

    private AlphaSkiaLinux() {
    }

    @Override
    protected String[] getJavaResources() {
        return new String[] {
            "/native/linux-" + getCurrentArchitecture() + "/libalphaskiajni.so"
        };
    }
}
