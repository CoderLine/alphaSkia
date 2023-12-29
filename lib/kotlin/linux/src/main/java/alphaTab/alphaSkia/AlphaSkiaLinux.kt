package alphaTab.alphaSkia

/**
 * The alphaSkia initializer for Linux.
 */
class AlphaSkiaLinux private constructor() : AlphaSkiaJre() {
    override fun getJavaResources(): Array<String> {
        return arrayOf(
            "/native/linux-" + getCurrentArchitecture() + "/libalphaskiajni.so"
        )
    }

    companion object {
        /**
         * The alphaSkia initializer for Linux.
         */
        @JvmStatic
        val INSTANCE = AlphaSkiaLinux()
    }
}

