package alphaTab.alphaSkia

/**
 * The alphaSkia initializer for Windows.
 */
class AlphaSkiaWindows private constructor() : AlphaSkiaJre() {
    override fun getJavaResources(): Array<String> {
        return arrayOf(
            "/native/win-" + getCurrentArchitecture() + "/libalphaskiajni.dll"
        )
    }

    companion object {
        /**
         * The alphaSkia initializer for Windows.
         */
        @JvmStatic
        val INSTANCE = AlphaSkiaWindows()
    }
}
