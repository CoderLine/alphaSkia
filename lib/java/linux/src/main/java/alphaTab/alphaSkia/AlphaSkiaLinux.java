package alphaTab.alphaSkia;

/**
 * This class contains the information about the Linux runtime dependencies to use with AlphaSkiaPlatform.
 */
public class AlphaSkiaLinux {
    /**
     * The native libraries needed to run alphaSkia.
     */
    public static final String[] libraries = {
            "/native/linux-" + getCurrentArchitecture() + "/libalphaskiajni.so"
    };

    private static String getCurrentArchitecture() {
        var jarch = System.getProperty("os.arch");
        return switch (jarch) {
            case "x86", "i368", "i486", "i586", "i686" -> "x86";
            case "x86_64", "amd64" -> "x64";
            case "arm" -> "arm";
            case "aarch64" -> "arm64";
            default -> jarch;
        };
    }
}
