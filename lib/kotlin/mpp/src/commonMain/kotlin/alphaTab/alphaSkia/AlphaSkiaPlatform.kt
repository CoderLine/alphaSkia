package alphaTab.alphaSkia

/**
 * This class provides a way of resolving and loading the native libraries required
 * by alphaSkia.
 */
abstract class AlphaSkiaPlatform {
    companion object {
        @JvmStatic
        private var nativeLibLoaded: Boolean = false

        /**
         * Gets a value indicating whether the native libraries required by alphaSkia were loaded.
         *
         * @return {@code true} if the libraries were loaded, otherwise {@code false}.
         */
        @JvmStatic
        fun isNativeLibLoaded(): Boolean {
            return nativeLibLoaded
        }

        /**
         * Sets a value indicating whether the native libraries required by alphaSkia were loaded.
         */
        @JvmStatic
        protected fun setNativeLibLoaded() {
            nativeLibLoaded = true
        }


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
     * Initializes the alphaSkia platform specifics using the given context.
     */
    abstract fun initialize()
}

