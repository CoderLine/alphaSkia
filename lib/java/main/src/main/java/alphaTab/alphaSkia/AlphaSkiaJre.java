package alphaTab.alphaSkia;

import java.io.File;
import java.io.IOException;
import java.io.InputStream;
import java.net.URL;
import java.nio.file.Files;
import java.nio.file.Path;
import java.nio.file.Paths;

/**
 * A base class for initializing JRE based platforms.
 */
public abstract class AlphaSkiaJre extends AlphaSkiaPlatform {

    /**
     * Gets the native libraries needed to run alphaSkia.
     * @return The list of Java Resources to load as libraries.
     */
    protected abstract String[] getJavaResources();

    @SuppressWarnings("UnsafeDynamicallyLoadedCode")
    @Override
    public void inititalize() throws IOException {
        String[] libraries = getJavaResources();

        Path tmpDir = Files.createTempDirectory("alphaskia");
        Class<?> thisClass = getClass();

        for (String library : libraries) {
            File tmpLib = tmpDir.resolve(Paths.get(library).getFileName().toString()).toFile();
            tmpLib.deleteOnExit();

            URL resource = thisClass.getResource(library);
            if (resource == null) {
                throw new IOException("Resource " + library + " not found");
            }

            try (InputStream in = resource.openStream()) {
                Files.copy(in, tmpLib.toPath());
            }
            System.load(tmpLib.getAbsolutePath());
        }
        setNativeLibLoaded();
    }
}
