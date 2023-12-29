package alphaTab.alphaSkia

internal class AlphaSkiaData : AlphaSkiaNative {
    constructor(handle: Long) : super(handle)

    constructor(raw: ByteArray) : super(allocateCopy(raw))

    companion object {
        @JvmStatic
        external fun allocateCopy(raw: ByteArray): Long
    }


    external override fun close()
    external fun toArray(): ByteArray
}
