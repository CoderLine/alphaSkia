package alphaTab.alphaSkia;

import java.io.IOException
import java.nio.file.Files
import java.nio.file.Paths

/**
 * A base class for initializing JRE based platforms.
 */
abstract class AlphaSkiaJre : AlphaSkiaPlatform() {
    companion object {
        /**
         * Maps the current CPU architecture to the alphaSkia internal architecture.
         * @return The alphaSkia architecture key.
         */
        @JvmStatic
        protected fun getCurrentArchitecture(): String {
            val jarch = System.getProperty("os.arch")
            return when (jarch) {
                "x86", "i368", "i486", "i586", "i686" -> "x86"
                "x86_64", "amd64" -> "x64"
                "arm" -> "arm"
                "aarch64" -> "arm64"
                else -> throw IllegalStateException ("Unsupported architecture $jarch")
            }
        }
    }

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
