package alphaTab.alphaSkia

internal class AlphaSkiaData(raw: ByteArray) :
    AlphaSkiaNative(NativeMethods.alphaskiaDataNewCopy(raw)) {

    override fun close() {
        NativeMethods.alphaskiaDataFree(this.handle)
    }

    fun toArray(): ByteArray {
        return NativeMethods.alphaskiaDataGetData(this.handle)
    }
}
