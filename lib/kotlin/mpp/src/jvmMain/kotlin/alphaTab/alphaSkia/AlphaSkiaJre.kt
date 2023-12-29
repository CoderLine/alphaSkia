package alphaTab.alphaSkia;

import java.io.IOException
import java.nio.file.Files
import java.nio.file.Paths

/**
 * A base class for initializing JRE based platforms.
 */
abstract class AlphaSkiaJre : AlphaSkiaPlatform() {
    /**
     * Gets the native libraries needed to run alphaSkia.
     * @return The list of Java Resources to load as libraries.
     */
    protected abstract fun getJavaResources(): Array<String>

    @Suppress("UnsafeDynamicallyLoadedCode")
    override fun initialize() {
        val libraries = getJavaResources()

        val tmpDir = Files.createTempDirectory("alphaskia")
        val thisClass = javaClass

        for (library in libraries) {
            val tmpLib = tmpDir.resolve(Paths.get(library).fileName.toString()).toFile()
            tmpLib.deleteOnExit();

            val resource = thisClass.getResource(library)
                ?: throw IOException("Resource $library not found")

            resource.openStream().use {
                Files.copy(it, tmpLib.toPath())
            }

            System.load(tmpLib.absolutePath);
            setNativeLibLoaded()
        }
    }
}
