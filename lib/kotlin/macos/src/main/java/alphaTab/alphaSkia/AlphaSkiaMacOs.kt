package alphaTab.alphaSkia

/**
 * The alphaSkia initializer for macOS.
 */
class AlphaSkiaMacOs private constructor() : AlphaSkiaJre() {
    override fun getJavaResources(): Array<String> {
        return arrayOf(
            "/native/macos-" + getCurrentArchitecture() + "/libalphaskiajni.dylib"
        )
    }

    companion object {
        /**
         * The alphaSkia initializer for Linux.
         */
        @JvmStatic
        val INSTANCE = AlphaSkiaMacOs()
    }
}

