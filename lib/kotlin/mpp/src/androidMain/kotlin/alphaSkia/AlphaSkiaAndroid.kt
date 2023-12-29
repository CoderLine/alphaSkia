package alphaTab.alphaSkia

/**
 * The alphaSkia initializer for Android.
 */
class AlphaSkiaAndroid : AlphaSkiaPlatform() {
    companion object {
        @JvmStatic
        val INSTANCE: AlphaSkiaAndroid = AlphaSkiaAndroid()
    }

    override fun initialize() {
        System.loadLibrary("alphaskiajni");
        setNativeLibLoaded();
    }
}

