package alphaTab.alphaSkia

/**
 * Represents a final rendered image.
 */
class AlphaSkiaImage internal constructor(handle: Long) : AlphaSkiaNative(handle) {
    /**
     * Gets the width of the image.
     *
     * @return the width of the image
     */
    external fun getWidth(): Int

    /**
     * Gets the height of the image.
     *
     * @return the height of the image
     */
    external fun getHeight(): Int

    @Override
    external override fun close()

    /**
     * Reads the raw pixel data of this image as byte array.
     *
     * @return A copy of the raw pixel data.
     */
    external fun readPixels(): ByteArray?

    /**
     * Encodes the image to a PNG.
     *
     * @return The raw PNG bytes for further usage.
     */
    external fun toPng(): ByteArray?

    companion object {
        /**
         * Decodes the given bytes into an image using supported image formats like PNG.
         * @param bytes The raw image bytes.
         * @return The decoded image or {@code null} if the image could not be decoded.
         */
        @JvmStatic
        fun decode(bytes: ByteArray): AlphaSkiaImage? {
            val handle = allocateDecoded(bytes)
            return if (handle == 0L) null else AlphaSkiaImage(handle)
        }

        @JvmStatic
        private external fun allocateDecoded(bytes: ByteArray): Long

        /**
         * Creates an image from the raw pixels assuming the default internal pixel format of alphaSkia.
         * @param width The width of the image in pixels.
         * @param height The height of the image in pixels.
         * @param pixels The raw pixel bytes.
         * @return The decoded image or {@code null} if the creation of the image from the pixels failed.
         */
        @JvmStatic
        fun fromPixels(width: Int, height: Int, pixels: ByteArray): AlphaSkiaImage? {
            val handle = createFromPixels(width, height, pixels)
            return if (handle == 0L) null else AlphaSkiaImage(handle)
        }

        @JvmStatic
        private external fun createFromPixels(width: Int, height: Int, pixels: ByteArray): Long
    }
}
