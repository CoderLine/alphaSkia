package alphaTab.alphaSkia

/**
 * Lists all vertical text baseline alignments which can be used to draw text.
 */
enum class AlphaSkiaTextBaseline(val value: alphaskia_text_baseline_t) {
    /**
     * The text is drawn using the alphabetic baseline.
     */
    ALPHABETIC(0),

    /**
     * The text is top-aligned to the provided position.
     */
    TOP(1),

    /**
     * The test is middle-aligned (vertically centered) to the provided position.
     */
    MIDDLE(2),

    /**
     * The text is bottom-aligned to the provided position.
     */
    BOTTOM(3);
}
