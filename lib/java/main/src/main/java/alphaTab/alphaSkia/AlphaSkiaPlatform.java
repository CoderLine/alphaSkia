package alphaTab.alphaSkia;

import java.io.IOException;

/**
 * This class provides a way of resolving and loading the native libraries required
 * by alphaSkia.
 */
public abstract class AlphaSkiaPlatform {
    private static boolean nativeLibLoaded = false;

    /**
     * Gets a value indicating whether the native libraries required by alphaSkia were loaded.
     *
     * @return {@code true} if the libraries were loaded, otherwise {@code false}.
     */
    public static boolean isNativeLibLoaded() {
        return nativeLibLoaded;
    }

    /**
     * Sets a value indicating whether the native libraries required by alphaSkia were loaded.
     */
    protected static void setNativeLibLoaded() {
        AlphaSkiaPlatform.nativeLibLoaded = true;
    }

    /**
     * Initializes the alphaSkia platform specifics using the given context.
     * @throws IOException Thrown if some IO related errors occur while loading the libraries.
     */
    public abstract void initialize() throws IOException;

    /**
     * Maps the current CPU architecture to the alphaSkia internal architecture.
     * @return The alphaSkia architecture key.
     */
    protected static String getCurrentArchitecture() {
        var jarch = System.getProperty("os.arch");
        return switch (jarch) {
            case "x86", "i368", "i486", "i586", "i686" -> "x86";
            case "x86_64", "amd64" -> "x64";
            case "arm" -> "arm";
            case "aarch64" -> "arm64";
            default -> throw new IllegalStateException("Unsupported architecture " + jarch);
        };
    }
}

