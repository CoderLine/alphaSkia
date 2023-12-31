package alphaTab.alphaSkia

/**
 * The alphaSkia initializer for Android.
 */
class AlphaSkiaAndroid : AlphaSkiaPlatform() {
    companion object {
        @JvmField
        val INSTANCE: AlphaSkiaAndroid = AlphaSkiaAndroid()
    }

    override fun initialize() {
        System.loadLibrary("alphaskiajni");
        setNativeLibLoaded();
    }
}

