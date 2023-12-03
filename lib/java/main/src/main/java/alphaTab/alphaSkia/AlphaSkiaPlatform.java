package alphaTab.alphaSkia;

import java.io.File;
import java.io.FileNotFoundException;
import java.io.IOException;
import java.io.InputStream;
import java.net.URL;
import java.nio.file.Files;
import java.nio.file.Path;
import java.nio.file.Paths;

/**
 * This class provides a way of resolving and loading the native libraries required
 * by alphaSkia.
 */
public final class AlphaSkiaPlatform {
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
     * Sets whether the internal tracing of alphaSkia should be enabled
     */
    public static native void setTracingEnabled(boolean isEnabled);

    /**
     * Initiates the loading of native libraries from the given file paths assuming they are the correct ones for alphaSkia.
     *
     * @param nativeLibraryPaths The paths to the files to load.
     * @throws IOException Thrown if any of the paths provided point to a non-existent file.
     */
    public static void loadLibrary(String[] nativeLibraryPaths) throws IOException {
        for (String library : nativeLibraryPaths) {
            var file = new File(library);
            if (!file.exists()) {
                throw new FileNotFoundException("Could not find file: " + library);
            }
            System.load(library);
        }
        nativeLibLoaded = true;
    }

    /**
     * Initiates loading of native libraries from the given platform information class shipped as part of
     * individual alphaSkia platform packages.
     *
     * @param platformInfo The platform info class like {@code net.alphatab.alphaskia.ALphaSkiaWindows} provided
     *                     through individual platform packages.
     * @throws IOException              Thrown if the resources expected to be provided by the platform package could not be found or extracted.
     * @throws IllegalArgumentException Thrown if the provided platform info class is not compliant with the needs of alphaSkia. Indicates typically an incompatibility or a wrong class being provided.
     */
    public static void loadLibrary(Class<?> platformInfo) throws IOException {
        try {
            String[] libraries = (String[]) platformInfo.getDeclaredField("libraries").get(null);

            Path tmpDir = Files.createTempDirectory("alphaskia");
            for (String library : libraries) {
                URL resource = platformInfo.getResource(library);
                if (resource == null) {
                    throw new IOException("Resource " + library + " not found");
                }
                File tmpLib = tmpDir.resolve(Paths.get(library).getFileName().toString()).toFile();
                tmpLib.deleteOnExit();
                try (InputStream in = resource.openStream()) {
                    Files.copy(in, tmpLib.toPath());
                }
                System.load(tmpLib.getAbsolutePath());
            }
            nativeLibLoaded = true;
        } catch (NoSuchFieldException | IllegalAccessException e) {
            throw new IllegalArgumentException("Type " + platformInfo.getName() + " seems not to be a valid AlphaSkia Target", e);
        }
    }

    private AlphaSkiaPlatform() {
    }
}
