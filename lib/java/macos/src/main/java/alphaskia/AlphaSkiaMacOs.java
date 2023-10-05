package alphaskia;

public class AlphaSkiaMacOs {
    public static final String[] libraries = {
            "native/macos-" + getCurrentArchitecture() + "/libAlphaSkiaJni.so"
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
