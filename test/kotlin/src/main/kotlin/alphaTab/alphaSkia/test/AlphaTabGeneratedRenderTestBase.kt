package alphaTab.alphaSkia.test

import alphaTab.alphaSkia.AlphaSkiaTextAlign
import alphaTab.alphaSkia.AlphaSkiaTextBaseline
import alphaTab.alphaSkia.AlphaSkiaTypeface
import java.nio.file.Files
import java.nio.file.Path
import java.util.Locale
import java.util.TreeMap

open class AlphaTabGeneratedRenderTestBase {
    companion object {
        @JvmStatic
        var renderScale = 1f
        @JvmStatic
        lateinit var musicTypeface: AlphaSkiaTypeface

        const val musicFontSize: Float = 34f

        @JvmStatic
        var textAlign = AlphaSkiaTextAlign.LEFT
        @JvmStatic
        var textBaseline = AlphaSkiaTextBaseline.TOP
        @JvmStatic
        lateinit var typeface: AlphaSkiaTypeface
        @JvmStatic
        var fontSize = 12f

        private val customTypefaces: MutableMap<String, AlphaSkiaTypeface> =
            TreeMap(java.lang.String.CASE_INSENSITIVE_ORDER)

        private fun customTypefaceKey(
            fontFamily: String,
            isBold: Boolean,
            isItalic: Boolean
        ): String {
            return fontFamily.lowercase(Locale.getDefault()) + "_" + isBold + "_" + isItalic
        }

        @JvmStatic
        protected fun getTypeface(
            fontFamily: String,
            isBold: Boolean,
            isItalic: Boolean
        ): AlphaSkiaTypeface {
            val key = customTypefaceKey(
                fontFamily,
                isBold,
                isItalic
            )
            return customTypefaces[key]
                ?: throw IllegalStateException("Unknown font requested: $key")
        }

        fun loadTypeface(
            name: String,
            isBold: Boolean,
            isItalic: Boolean,
            filePath: Path
        ): AlphaSkiaTypeface {
            val key = customTypefaceKey(name, isBold, isItalic)
            println("Loading typeface $key from $filePath")
            val data = Files.readAllBytes(filePath)
            println("Read " + data.size + " bytes from file, decoding typeface")
            val typeface = AlphaSkiaTypeface.register(data)
                ?: throw IllegalStateException("Could not create typeface from data")
            customTypefaces[key] = typeface
            return typeface
        }
    }
}
