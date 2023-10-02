package net.alphatab.alphaskia;

import java.io.File;
import java.io.FileNotFoundException;
import java.io.IOException;
import java.io.InputStream;
import java.net.URL;
import java.nio.file.Files;
import java.nio.file.Path;
import java.nio.file.Paths;

public final class AlphaSkiaPlatform {
    private static boolean nativeLibLoaded = false;

    public static boolean isNativeLibLoaded() {
        return nativeLibLoaded;
    }

    public static void loadLibrary(String[] nativeLibraryPaths) throws IOException {
        for(String library : nativeLibraryPaths) {
            var file = new File(library);
            if(!file.exists()){
                throw new FileNotFoundException("Could not find file: " + library);
            }
            System.load(library);
        }
        nativeLibLoaded = true;
    }

    public static void loadLibrary(Class<?> platformInfo) throws IOException {
        try {
            String[] libraries = (String[]) platformInfo.getDeclaredField("libraries").get(null);

            Path tmpDir = Files.createTempDirectory("alphaSkia");
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
