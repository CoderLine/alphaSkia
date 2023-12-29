package alphaTab.alphaSkia;

/**
 * The base class for AlphaSkia objects wrapping native Skia objects.
 */
abstract class AlphaSkiaNative internal constructor(internal var handle: Long) : AutoCloseable {
    companion object {
        init {
            if (!AlphaSkiaPlatform.isNativeLibLoaded()) {
                throw IllegalStateException("Initialize the alphaSkia platform first")
            }
        }
    }

    abstract override fun close()
}
