package alphaTab.alphaSkia

import kotlin.jvm.JvmStatic

/**
 * Represents a typeface to draw text.
 */
class AlphaSkiaTypeface private constructor(handle: Long, data: AlphaSkiaData?) :
    AlphaSkiaNative(handle) {

    private val _data: AlphaSkiaData? = data
    private var _familyName: String? = null

    /**
     * Gets the name of the font family of this typeface.
     *
     * @return The name of the font family of this typeface
     */
    val familyName: String
        get() {
            var familyName = this._familyName
            if (familyName == null) {
                familyName = NativeMethods.alphaskiaTypefaceGetFamilyName(this.handle)
                this._familyName = familyName
            }
            return familyName
        }

    /**
     * Gets a value indicating whether the typeface is bold.
     * @return true if the font is bold, otherwise false.
     */
    val bold: Boolean
        get() = NativeMethods.alphaskiaTypefaceIsBold(this.handle)

    /**
     * Gets a value indicating whether the typeface is italic.
     * @return true if the font is italic, otherwise false.
     */
    val italic: Boolean
        get() = NativeMethods.alphaskiaTypefaceIsItalic(this.handle)

    override fun close() {
        _data?.close()
        NativeMethods.alphaskiaTypefaceFree(this.handle)
        handle = 0
    }

    companion object {
        /**
         * Register a new custom font from the given binary data containing the data of a font compatible with Skia (e.g. TTF).
         *
         * @param data The raw binary data of the font.
         * @return The loaded typeface to use for text rendering or {@code null} if the loading failed.
         */
        @JvmStatic
        fun register(data: ByteArray): AlphaSkiaTypeface? {
            val nativeData = AlphaSkiaData(data)
            val typeface = NativeMethods.alphaskiaTypefaceRegister(nativeData.handle)
            if (typeface == 0L) {
                nativeData.close()
                return null
            }
            return AlphaSkiaTypeface(typeface, nativeData)
        }

        /**
         * Creates a typeface using the provided information.
         *
         * @param name   The name of the typeface.
         * @param bold   Whether the bold version of the typeface should be loaded.
         * @param italic Whether the italic version of the typeface should be loaded.
         * @return The typeface if it can be found in the already loaded fonts or the system fonts, otherwise {@code null}.
         */
        @JvmStatic
        fun create(name: String, bold: Boolean, italic: Boolean): AlphaSkiaTypeface? {
            val typeface = NativeMethods.alphaskiaTypefaceMakeFromName(name, bold, italic)
            if (typeface == 0L) {
                return null
            }

            return AlphaSkiaTypeface(typeface, null)
        }
    }
}
