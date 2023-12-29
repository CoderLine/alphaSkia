package alphaTab.alphaSkia

/**
 * Lists all known color type with which AlphaSkia creates images.
 */
enum class AlphaSkiaColorType(val value: Int) {
    // NOTE: we internally use kN32_SkColorType which is either  kBGRA_8888_SkColorType or kRGBA_8888_SkColorType
    // hence we don't need to expose the other SkColorType values

    /**
     * pixel with 8 bits for red, green, blue, alpha; in 32-bit word
     */
    RGBA_8888(4),

    /**
     * pixel with 8 bits for blue, green, red, alpha; in 32-bit word
     */
    BGRA_8888(6);

    companion object {
        @JvmStatic
        fun fromValue(value: Int): AlphaSkiaColorType? {
            if (value == RGBA_8888.value) {
                return RGBA_8888
            } else if (value == BGRA_8888.value) {
                return BGRA_8888
            }
            return null
        }
    }
}
