package alphaTab.alphaSkia

import kotlin.jvm.JvmStatic

/**
 * This class provides a way of resolving and loading the native libraries required
 * by alphaSkia.
 */
abstract class AlphaSkiaPlatform {
    companion object {
        @JvmStatic
        private var nativeLibLoaded: Boolean = !NativeMethods.requiresNativeLibLoading

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
    }

    /**
     * Initializes the alphaSkia platform specifics using the given context.
     */
    abstract fun initialize()
}

